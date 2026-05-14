using DotchatServer.src.Application.DTOs.EmailModels;
using DotchatServer.src.Application.DTOs.Emails;
using DotchatServer.src.Application.Extensions;
using DotchatServer.src.Application.Interfaces;
using DotchatServer.src.Application.Services;
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

        _ = services.AddSingleton<IRazorEngine, RazorEngine>();
        _ = services.AddSingleton<ITemplateFactory<Email>, TemplateFactory<Email>>((services) => new TemplateFactory<Email>
            (
                razorEngine: services.GetRequiredService<IRazorEngine>(),
                resxManager: ResxManager.From(env),
                AppPath.From(env),
                baseFolderPath: "EmailTemplates", //TODO: Remove this make it either configurable or only use AppPath, Document 
                new Func<string?, string, Email>((subject, body) => new Email(subject, body)
            )));

        _ = services.AddSingleton<ITemplateFactory<string> >((services) => new TemplateFactory<string>
            (
                razorEngine: services.GetRequiredService<IRazorEngine>(),
                resxManager: ResxManager.From(env),
                AppPath.From(env),
                baseFolderPath: "EmailConfirmationTemplates", //TODO: Remove this make it either configurable or only use AppPath, Document 
                new Func<string?, string, string>((_, body) => body)
            ));

        _ = services.AddSingleton<IEmailClient, EmailClient>(services => new EmailClient(configuration.GetValue<bool>("SendEmailToFakeSMPT") 
            ? configuration.GetSection("EmailSettingsDev").Get<EmailOptions>()! 
            : configuration.GetSection("EmailSettingsProd").Get<EmailOptions>()!));
        _ = services.AddSingleton<AppPath>(provider => AppPath.From(provider.GetRequiredService<IWebHostEnvironment>()));

        _ = services.AddDbContextPoolWithWarmup<AppDbContext, DbContextWarmupUtility>(
            opt => opt.UseNpgsql(connectionString: configuration.GetConnectionString("PostgreSQL")));

        _ = services.AddScoped<IAuthRepository, AuthRepository>();
    }
}