namespace DotchatServer.src.Application.DTOs.JwtModels;

public sealed record JwtClientData(string RefreshToken, string AccessToken, TimeSpan Expiery);