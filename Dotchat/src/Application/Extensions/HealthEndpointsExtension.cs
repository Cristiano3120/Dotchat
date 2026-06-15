using System.Text.Json;
using DotchatShared.src.Constants;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;

namespace DotchatServer.src.Application.Extensions;

public static class HealthEndpointsExtension
{
    public static IServiceCollection AddHealthChecks(this IServiceCollection services, 
        string redisConnectionString, string postgresqlConnectionString)
    {
        _ = services.AddHealthChecks()
            .AddNpgSql(connectionString: postgresqlConnectionString, name: "postgres", tags: ["ready"])
            .AddRedis(redisConnectionString: redisConnectionString, name: "redis", tags: ["ready"]);
    
        return services;
    }

    public static IApplicationBuilder MapHealthEndpoints(this WebApplication app)
    {
        _ = app.MapHealthChecks(Endpoints.HealthEndpoints.LivenessEndpoint, new HealthCheckOptions
        {
            Predicate = _ => false // process responds = alive, no checks executed
        });

        _ = app.MapHealthChecks(Endpoints.HealthEndpoints.ReadinessEndpoint, new HealthCheckOptions
        {
            Predicate = c => c.Tags.Contains("ready"),
            ResponseWriter = async (ctx, report) =>
            {
                ctx.Response.ContentType = "application/json";
                string json = JsonSerializer.Serialize(new
                {
                    status = report.Status.ToString(),
                    checks = report.Entries.Select(e => new { e.Key, status = e.Value.Status.ToString() })
                });
                await ctx.Response.WriteAsync(json);
            }
        });

        return app;
    }
}
