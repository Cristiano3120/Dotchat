using DotchatServer.src.Application.Interfaces;
using DotchatServer.src.Infrastructure.Persistence;
using DotchatServer.src.Infrastructure.Persistence.Repos;
using Microsoft.EntityFrameworkCore;
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

        _ = services.AddDbContextPool<AppDbContext>(opt => opt.UseNpgsql(
            connectionString: configuration.GetConnectionString("PostgreSQL")));                   

        _ = services.AddScoped<IAuthRepository, AuthRepository>();
    }
}