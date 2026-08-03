namespace DotchatServer.src.Application.DTOs;

/// <remarks>Keep the key secure to prevent unauthorized token creation.</remarks>
/// <param name="AccessTokenExpiry">Token expiration in minutes.</param>
/// <param name="RefreshTokenExpiry">Token expiration in days. </param>
public sealed record JwtSettings(string Key, string Issuer, string Audience, int AccessTokenExpiry, int RefreshTokenExpiry);