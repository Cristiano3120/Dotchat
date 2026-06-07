namespace DotchatServer.src.Application.DTOs.JwtModels;

/// <remarks>Keep the key secure to prevent unauthorized token creation.</remarks>
/// <param name="Expiry">Token expiration in minutes.</param>
public sealed record JwtSettings(string Key, string Issuer, string Audience, int Expiry);