using Destructurama;
using DotchatServer.src.Application.DTOs;
using DotchatServer.src.Application.Extensions;
using DotchatServer.src.Application.Interfaces;
using DotchatServer.src.Constants;
using DotchatServer.src.Infrastructure;
using DotNetEnv;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.IdentityModel.Tokens;
using RedisRateLimiting;
using Serilog;
using Serilog.Events;
using StackExchange.Redis;
using System.Text;

namespace DotchatServer.src;

public static class Program
{
    public static async Task Main()
    {
        Log.Logger = new LoggerConfiguration()
           .Enrich.FromLogContext()
           .Destructure.UsingAttributes()
           .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {NewLine}{Exception}")
           .MinimumLevel.Information()
           .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
           .MinimumLevel.Override("System", LogEventLevel.Warning)
           .CreateLogger();

        WebApplicationBuilder builder = WebApplication.CreateBuilder();
        IEnumerable<KeyValuePair<string, string>> envVals = Env.Load();

        _ = builder.Services.AddControllers();
        _ = builder.Services.AddOpenApi();
        _ = builder.Services.AddRateLimiter(options =>
        {
            _ = options.AddPolicy(policyName: RateLimitPolicies.Auth,
                httpContext => RedisRateLimitPartition.GetSlidingWindowRateLimiter(
                    partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? httpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault(),
                    factory: _ => new RedisSlidingWindowRateLimiterOptions
                    {
                        PermitLimit = 5,
                        Window = TimeSpan.FromMinutes(1),
                        ConnectionMultiplexerFactory = () => httpContext.RequestServices.GetRequiredService<IConnectionMultiplexer>()
                    }
    )
);

            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
        });

        JwtSettings jwtSettings = builder.Configuration.GetSection("JwtSettings").Get<JwtSettings>()
            ?? throw new InvalidOperationException("Failed to bind JwtSettings from configuration.");

        _ = builder.Services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwtSettings.Issuer,
                ValidAudience = jwtSettings.Audience,
                IssuerSigningKey = new SymmetricSecurityKey(key: Encoding.UTF8.GetBytes(jwtSettings.Key)),
                ClockSkew = TimeSpan.Zero
            };
        });

        _ = builder.Services.AddAuthorization();


        builder.Services.AddInfrastructureServices(envVals, configuration: builder.Configuration);
        builder.Services.AddApplicationServices(jwtSettings, workerID: builder.Configuration.GetValue<int>("WorkerID"));

        WebApplication app = builder.Build();

        _ = app.UseForwardedHeaders(new ForwardedHeadersOptions
        {
            ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
        });

        _ = app.MapOpenApi();
        _ = app.UseRateLimiter();
        _ = app.UseRouting();
        _ = app.UseAuthentication();
        _ = app.UseAuthorization();
        _ = app.MapControllers();

        //Warmup every service that implements IWarmable. Hashing Services are an example for this
        IEnumerable<IWarmable> warmables = app.Services.GetServices<IWarmable>();
        await Task.WhenAll(warmables.Select(w => w.WarmupAsync()));

        await app.RunAsync(app.Configuration.GetConnectionString("WebAdress"));
    }
}