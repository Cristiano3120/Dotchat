using DotchatServer.src.Application.DTOs.JwtModels;

namespace DotchatServer.src.Application.Interfaces;

public interface IJwtService
{
    public JwtClientData GenerateToken(long userId, string email);
}