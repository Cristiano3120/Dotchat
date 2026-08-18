using DotchatServer.src.Application.DTOs.EmailModels;
using DotchatServer.src.Application.Enums;
using DotchatServer.src.Application.Extensions;
using DotchatServer.src.Application.Interfaces;
using DotchatServer.src.Application.Services;
using DotchatServer.src.Core.Config;
using DotchatServer.src.Core.Interfaces;
using DotchatServer.src.Infrastructure.Persistence;
using DotchatServer.src.Infrastructure.Persistence.Repos;
using DotchatShared.src.DTOs;
using DotchatShared.src.Interfaces;
using DotchatShared.src.Services;
using Microsoft.EntityFrameworkCore;
using RazorEngineCore;
using StackExchange.Redis;

namespace DotchatServer.src.Infrastructure;

internal static class InfrastructureServiceExtensions
{
    public static void AddInfrastructureServices(this IServiceCollection services,
        IWebHostEnvironment env,
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

        _ = services.AddSingleton<IRedisCache, RedisCache>();
        _ = services.AddSingleton<IRazorEngine, RazorEngine>();
        _ = services.AddSingleton<ITemplateFactory, TemplateFactory>(
            (services) =>
            {
                IRazorEngine razorEngine = services.GetRequiredService<IRazorEngine>();
                AppPath appPath = AppPath.From(env.ContentRootPath).Src().Go("HtmlTemplates");
                return new TemplateFactory(razorEngine, appPath);
            });

        _ = services.AddSingleton<IEmailClient, EmailClient>(services => new EmailClient(configuration.GetValue<bool>("SendEmailToFakeSMPT")
            ? configuration.GetSection("EmailSettingsDev").Get<EmailConfig>()!
            : configuration.GetSection("EmailSettingsProd").Get<EmailConfig>()!));
        _ = services.AddSingleton<AppPath>(provider => AppPath.From(provider.GetRequiredService<IWebHostEnvironment>().ContentRootPath));

        _ = services.AddDbContextPoolWithWarmup<AppDbContext, DbContextWarmupUtility>(
            opt => opt.UseNpgsql(connectionString: configuration.GetConnectionString("PostgreSQL")));

        _ = services.AddScoped<IAuthRepository, AuthRepository>();
    }
}