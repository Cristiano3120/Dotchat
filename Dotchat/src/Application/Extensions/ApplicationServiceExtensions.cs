using DotchatServer.src.Application.DTOs;
using DotchatServer.src.Application.Enums;
using DotchatServer.src.Application.Interfaces;
using DotchatServer.src.Application.Interfaces.Security;
using DotchatServer.src.Application.Services;

namespace DotchatServer.src.Application.Extensions;

public static class ApplicationServiceExtensions
{
    public static void AddApplicationServices(this IServiceCollection services, JwtSettings jwtSettings, int workerID)
    {
        _ = services.AddSingleton<IJwtService, JwtService>((_) => new JwtService(jwtSettings));
        _ = services.AddSingleton<SnowflakeGenerator>((_) => new SnowflakeGenerator(workerID));
        _ = services.AddKeyedSingletonWithWarmup<IHashingService, Argon2Hasher>(HashingAlgorithm.Argon2);
        _ = services.AddScoped<AuthService>();
    }
}