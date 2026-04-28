namespace DotchatServer.src.Application.DTOs.JwtModels;

/// <summary>
/// Represents the configuration settings required for issuing and validating JSON Web Tokens (JWT) in authentication
/// scenarios.
/// </summary>
/// <remarks>Use this record to configure JWT authentication parameters for token generation and validation.
/// Ensure that the key is kept secure to prevent unauthorized token creation.</remarks>
/// <param name="Key">The secret key used to sign and validate JWT tokens.</param>
/// <param name="Issuer">The issuer identifier to be included in the JWT token. Typically represents the authentication server or authority.</param>
/// <param name="Audience">The intended audience for the JWT token. Specifies the recipients that the token is valid for.</param>
/// <param name="Expiery">The token expiration time, in minutes. Determines how long the issued JWT token remains valid.</param>
public sealed record JwtSettings(string Key, string Issuer, string Audience, int Expiery);