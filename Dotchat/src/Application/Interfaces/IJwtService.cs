using DotchatServer.src.Application.DTOs.JwtModels;

namespace DotchatServer.src.Application.Interfaces;

/// <summary>
/// Provides functionality for generating JSON Web Tokens (JWT) for user authentication and authorization.
/// </summary>
/// <remarks>This service is typically used to issue JWTs that can be consumed by clients for secure API access.
/// The generated tokens include standard claims such as subject, email, and a unique identifier.</remarks>
/// <param name="jwtSettings">The settings used to configure JWT generation, including the signing key, issuer, audience, and token expiration.</param>
internal interface IJwtService
{
    /// <summary>
    /// Generates a JSON Web Token (JWT) for the specified user with the provided email address.
    /// </summary>
    /// <remarks>The generated token is signed using the configured symmetric security key and is valid for
    /// the duration specified in the JWT settings. The caller is responsible for securely storing and transmitting the
    /// token.</remarks>
    /// <param name="userId">The unique identifier of the user for whom the token is generated.</param>
    /// <param name="email">The email address to include in the token's claims.</param>
    /// <returns>An object containing the generated JWT, Refresh Token and expiry. The token includes the user ID and email as claims.</returns>
    public JwtClientData GenerateToken(long userId, string email);
}