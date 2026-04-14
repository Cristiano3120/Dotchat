using DotchatServer.src.Application.Extensions;
using DotchatServer.src.Application.Interfaces;
using DotchatServer.src.Core.Interfaces;
using DotchatServer.src.Infrastructure.Persistence;
using DotchatServer.src.Infrastructure.Persistence.Repos;
using Microsoft.EntityFrameworkCore;
using RazorEngineCore;
using StackExchange.Redis;

namespace DotchatServer.src.Infrastructure;

public static class InfrastructureServiceExtensions
{
    public static void AddInfrastructureServices(this IServiceCollection services, 
        IEnumerable<KeyValuePair<string, string>> envVals,
        IConfiguration configuration)
    {
        _ = services.AddSingleton<IConnectionMultiplexer>((_) =>
        {
            ConfigurationOptions conf = new()
            {
                EndPoints = { envVals.FirstOrDefault(x => x.Key == "REDIS_ENDPOINT").Value },
                Password = envVals.FirstOrDefault(x => x.Key == "REDIS_PASSWORD").Value,
                AbortOnConnectFail = false,
            };

            return ConnectionMultiplexer.Connect(conf);
        });

        _ = services.AddSingleton<IRazorEngine, RazorEngine>();
        _ = services.AddSingleton<IEmailFactory, EmailFactory>();
        _ = services.AddSingleton<IEmailClient, EmailClient>();
        _ = services.AddSingleton<AppPath>(provider => AppPath.From(provider.GetRequiredService<IWebHostEnvironment>()));

        _ = services.AddDbContextPoolWithWarmup<AppDbContext, DbContextWarmupUtility>(
            opt => opt.UseNpgsql(connectionString: configuration.GetConnectionString("PostgreSQL")));

        _ = services.AddScoped<IAuthRepository, AuthRepository>();
    }
}