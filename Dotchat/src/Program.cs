using System.Diagnostics;
using System.Globalization;
using System.Text;

using Destructurama;
using DotchatServer.src.Application.DTOs.JwtModels;
using DotchatServer.src.Application.Extensions;
using DotchatServer.src.Application.Interfaces;
using DotchatServer.src.Application.Services;
using DotchatServer.src.Constants;
using DotchatServer.src.Core.Config;
using DotchatServer.src.Core.Entities;
using DotchatServer.src.Core.Extensions;
using DotchatServer.src.Infrastructure;
using DotchatShared.src.Constants;
using DotNetEnv;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using RedisRateLimiting;

using Serilog;
using Serilog.Events;
using StackExchange.Redis;

namespace DotchatServer.src;

public static class Program
{
    public static async Task Main()
    {
        Log.Logger = new LoggerConfiguration()
           .Enrich.FromLogContext()
           .Destructure.UsingAttributes()
           .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {NewLine}{Exception}")
           .MinimumLevel.Is(LogEventLevel.Verbose)
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
                ClockSkew = TimeSpan.Zero,
            };
        });

        _ = builder.Services.AddAuthorization();
        _ = builder.Services.AddHealthChecks(builder.Configuration.GetConnectionString("Redis")!, builder.Configuration.GetConnectionString("PostgreSQL")!);

        _ = builder.Services.AddOptions<AppSettings>().Bind(builder.Configuration.GetSection("AppSettings"));
        _ = builder.Services.AddOptions<ConfirmationEmailConfig>().Bind(builder.Configuration.GetSection("ConfirmationEmailConfig"));

        AppSettings appSettings = builder.Configuration.GetSection("AppSettings").Get<AppSettings>()
            ?? throw new InvalidOperationException("Failed to bind AppSettings from configuration.");

        _ = builder.Services.AddCoreServices();
        builder.Services.AddInfrastructureServices(builder.Environment, envVals, configuration: builder.Configuration);
        builder.Services.AddApplicationServices(builder.Environment, appSettings, jwtSettings, workerID: builder.Configuration.GetValue<int>("WorkerID"));
        _ = builder.Services.AddHttpContextAccessor();
        _ = builder.Services.AddHttpClient(HttpClientNames.HealthCheckClient, (httpClient) =>
        {
            UrlBuilder urlBuilder = UrlBuilder.Create(appSettings.WebAddress);
            httpClient.Timeout = TimeSpan.FromSeconds(10);
            httpClient.BaseAddress = new Uri(urlBuilder.AddUrl(Endpoints.HealthEndpoints.BaseHealthEndpoint).Build() + "/");//Slash neeeded for correct URL building if the slash is missing the path will be api/live instead of api/health/live...
        });
        //TODO: Mach nen pipeline warmup. Injecte HTTP CLient Factory und rufe den Health Endpoint auf
        WebApplication app = builder.Build();

        _ = app.Use(async (context, next) =>
        {
            if (context is not null)
            {
                Log.Information("Request: {Method} {Path}", context.Request.Method, context.Request.Path);
                Log.Information("RemoteIpAddress: {RemoteIpAddress}", context.Connection.RemoteIpAddress?.ToString() ?? "Unknown");
                Log.Information("UserAgent: {UserAgent}", context.Request.Headers.UserAgent.FirstOrDefault() ?? "Unknown");
            }

            await next(context);
        });

        _ = app.UseForwardedHeaders(new ForwardedHeadersOptions
        {
            ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
        });

        _ = app.UseRequestLocalization(new RequestLocalizationOptions
        {
            DefaultRequestCulture = new RequestCulture("en"),
            SupportedCultures = [new CultureInfo("de"), new CultureInfo("en")],
        });

        _ = app.MapOpenApi();
        _ = app.UseRateLimiter();
        _ = app.UseRouting();
        _ = app.UseAuthentication();
        _ = app.UseAuthorization();
        _ = app.MapControllers();
        _ = app.MapHealthEndpoints();

        //Warmup every service that implements IWarmable. Hashing Services are an example for this
        IEnumerable<IWarmable> warmables = app.Services.GetServices<IWarmable>();
        await Task.WhenAll(warmables.Select(w => w.WarmupAsync()));

        if (app.Services.GetRequiredService<IOptions<AppSettings>>().Value.WebAddress is not string webAddress)
        {
            Log.Fatal("WebAddress is not configured. Please set the WebAddress in the configuration to start the application.");
            return;
        }

        _ = app.Lifetime.ApplicationStarted.Register(() => app.Services.GetService<PipelineWarmupService>()?.WarmupAsync());
        await app.RunAsync(webAddress);
    }
}