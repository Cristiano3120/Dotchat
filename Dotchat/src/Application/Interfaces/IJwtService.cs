using DotchatServer.src.Application.DTOs;

namespace DotchatServer.src.Application.Interfaces;

public interface IJwtService
{
    public JwtClientData GenerateToken(long userId, string email);
}