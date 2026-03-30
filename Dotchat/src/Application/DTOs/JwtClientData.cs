namespace DotchatServer.src.Application.DTOs;

public sealed record JwtClientData(string RefreshToken, string AccessToken, TimeSpan Expiery);