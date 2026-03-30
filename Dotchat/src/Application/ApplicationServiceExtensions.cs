using DotchatServer.src.Application.DTOs;
using DotchatServer.src.Application.Services;

namespace DotchatServer.src.Application;

public static class ApplicationServiceExtensions
{
    public static void AddApplicationServices(this IServiceCollection services, JwtSettings jwtSettings, int workerID)
    {
        _ = services.AddSingleton<JwtService>((_) => new JwtService(jwtSettings));
        _ = services.AddSingleton<SnowflakeGenerator>((_) => new SnowflakeGenerator(workerID));
    }
}