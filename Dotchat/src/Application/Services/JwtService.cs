using DotchatServer.src.Application.DTOs;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace DotchatServer.src.Application.Services;

/// <summary>
/// Provides functionality for generating JSON Web Tokens (JWT) for user authentication and authorization.
/// </summary>
/// <remarks>This service is typically used to issue JWTs that can be consumed by clients for secure API access.
/// The generated tokens include standard claims such as subject, email, and a unique identifier.</remarks>
/// <param name="jwtSettings">The settings used to configure JWT generation, including the signing key, issuer, audience, and token expiration.</param>
public sealed class JwtService(JwtSettings jwtSettings)
{
    /// <summary>
    /// Generates a JSON Web Token (JWT) for the specified user with the provided email address.
    /// </summary>
    /// <remarks>The generated token is signed using the configured symmetric security key and is valid for
    /// the duration specified in the JWT settings. The caller is responsible for securely storing and transmitting the
    /// token.</remarks>
    /// <param name="userId">The unique identifier of the user for whom the token is generated.</param>
    /// <param name="email">The email address to include in the token's claims.</param>
    /// <returns>An object containing the generated JWT, Refresh Token and Expiery. The token includes the user ID and email as claims.</returns>
    public JwtClientData GenerateToken(long userId, string email)
    {
        SymmetricSecurityKey key = new(key: Encoding.UTF8.GetBytes(jwtSettings.Key));

        Claim[] claims =
        [
            new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, email),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        ];

        JwtSecurityToken token = new(
            issuer: jwtSettings.Issuer,
            audience: jwtSettings.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(jwtSettings.Expiery),
            signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256)
        );

        return new JwtClientData
        (
            AccessToken: new JwtSecurityTokenHandler().WriteToken(token),
            RefreshToken: GenerateRefreshToken(),
            Expiery : TimeSpan.FromMinutes(jwtSettings.Expiery)
        );
    }

    private static string GenerateRefreshToken()
    {
        byte[] randomNumber = new byte[64];
        using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }
    }
}