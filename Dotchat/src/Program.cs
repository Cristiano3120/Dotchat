using DotchatServer.src.Application.Interfaces.Security;
using DotchatServer.src.Constants;
using DotNetEnv;
using Microsoft.AspNetCore.HttpOverrides;
using RedisRateLimiting;
using StackExchange.Redis;
using System.Collections;

namespace DotchatServer.src;

public static class Program
{
    public static async Task Main()
    {
        WebApplicationBuilder builder = WebApplication.CreateBuilder();
        IEnumerable<KeyValuePair<string, string>> envVals = Env.Load();

        _ = builder.Services.AddSingleton<IConnectionMultiplexer>((_) =>
        {
            ConfigurationOptions conf = new()
            {
                EndPoints = { envVals.FirstOrDefault(x => x.Key == "REDIS_ENDPOINT").Value },
                Password = envVals.FirstOrDefault(x => x.Key == "REDIS_PASSWORD").Value,
                AbortOnConnectFail = false,
            };

            return ConnectionMultiplexer.Connect(conf);
        });

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

        WebApplication app = builder.Build();
        
        _ = app.UseForwardedHeaders(new ForwardedHeadersOptions
        {
            ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
        });

        _ = app.MapOpenApi();
        _ = app.UseRateLimiter();
        _ = app.MapControllers();

        await app.RunAsync(app.Configuration.GetConnectionString("WebAdress"));
    }
}