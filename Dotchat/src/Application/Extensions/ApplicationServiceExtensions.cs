using DotchatServer.src.Application.DTOs.JwtModels;
using DotchatServer.src.Application.Enums;
using DotchatServer.src.Application.Factories;
using DotchatServer.src.Application.Interfaces;
using DotchatServer.src.Application.Interfaces.Security;
using DotchatServer.src.Application.Services;
using DotchatServer.src.Core.Entities;

namespace DotchatServer.src.Application.Extensions;

public static class ApplicationServiceExtensions
{
    public static void AddApplicationServices(this IServiceCollection services, 
        IWebHostEnvironment env, 
        AppSettings appSettings,
        JwtSettings jwtSettings, 
        int workerID)
    {
        _ = services.AddKeyedSingletonWithWarmup<IHashingService, Argon2Hasher>(HashingAlgorithm.Argon2);
        _ = services.AddSingletonWithWarmup<TemplatePrecompilationService>();
        _ = services.AddSingleton<IJwtService, JwtService>((_) => new JwtService(jwtSettings));
        _ = services.AddSingleton<SnowflakeGenerator>((_) => new SnowflakeGenerator(workerID));
        _ = services.AddSingleton<ResendConfirmationEmailModelFactory>();
        _ = services.AddSingleton<EmailConfirmationStatusModelFactory>();
        _ = services.AddSingleton<EmailConfirmationFailedModelFactory>();
        _ = services.AddSingleton<VerificationEmailFactory>();
        _ = services.AddSingleton<ResxManager>((services) => ResxManager.From(env));
        _ = services.AddSingleton<IUrlBuilder, UrlBuilder>((services) => UrlBuilder.Create(appSettings.WebAddress));
        _ = services.AddScoped<IAuthService, AuthService>();
        _ = services.AddSingleton<PipelineWarmupService>();
    }
}