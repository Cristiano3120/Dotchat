using DotchatServer.src.Application.DTOs;
using DotchatServer.src.Application.DTOs.Emails;
using DotchatServer.src.Application.Enums;
using DotchatServer.src.Application.Extensions;
using DotchatServer.src.Application.Interfaces;
using DotchatServer.src.Application.Services;
using DotchatServer.src.Core.Config;
using DotchatServer.src.Core.Interfaces;
using DotchatServer.src.Infrastructure.Persistence;
using DotchatServer.src.Infrastructure.Persistence.Repos;
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
        _ = services.AddSingleton<ITemplateFactory<Email>, TemplateFactory<Email>>((services) => new TemplateFactory<Email>
            (
                razorEngine: services.GetRequiredService<IRazorEngine>(),
                resxManager: ResxManager.From(env),
                AppPath.From(env).Src().Go("EmailTemplates"),
                new Func<string?, string, Email>((subject, body) => new Email(subject ?? string.Empty, body)
            )));

        _ = services.AddKeyedSingleton<ITemplateFactory<HtmlTemplate>>(TemplateFactoryKey.Confirmation, (services, _)
            => CreateHtmlTemplateFactory(services, AppPath.From(env).Src().Go("EmailConfirmationTemplates")));

        _ = services.AddKeyedSingleton<ITemplateFactory<HtmlTemplate>>(TemplateFactoryKey.ResendConfirmation, (services, _)
            => CreateHtmlTemplateFactory(services, AppPath.From(env).Src().Go("EmailConfirmationResendTemplates")));

        _ = services.AddSingleton<IEmailClient, EmailClient>(services => new EmailClient(configuration.GetValue<bool>("SendEmailToFakeSMPT")
            ? configuration.GetSection("EmailSettingsDev").Get<EmailConfig>()!
            : configuration.GetSection("EmailSettingsProd").Get<EmailConfig>()!));
        _ = services.AddSingleton<AppPath>(provider => AppPath.From(provider.GetRequiredService<IWebHostEnvironment>()));

        _ = services.AddDbContextPoolWithWarmup<AppDbContext, DbContextWarmupUtility>(
            opt => opt.UseNpgsql(connectionString: configuration.GetConnectionString("PostgreSQL")));

        _ = services.AddScoped<IAuthRepository, AuthRepository>();
    }

    private static ITemplateFactory<HtmlTemplate> CreateHtmlTemplateFactory(IServiceProvider services, AppPath appPath)
    {
        return new TemplateFactory<HtmlTemplate>
        (
            razorEngine: services.GetRequiredService<IRazorEngine>(),
            resxManager: ResxManager.From(services.GetRequiredService<IWebHostEnvironment>()),
            appPath,
            new Func<string?, string, HtmlTemplate>((_, body) => new HtmlTemplate(body))
        );
    }
}