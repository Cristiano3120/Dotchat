using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using DotchatServer.src.Application.DTOs.JwtModels;
using DotchatServer.src.Application.Interfaces;

using Microsoft.IdentityModel.Tokens;

namespace DotchatServer.src.Application.Services;

internal sealed class JwtService(JwtSettings jwtSettings) : IJwtService
{
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
            expires: DateTime.UtcNow.AddMinutes(jwtSettings.Expiry),
            signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256)
        );

        return new JwtClientData
        (
            AccessToken: new JwtSecurityTokenHandler().WriteToken(token),
            RefreshToken: GenerateRefreshToken(),
            Expiry: TimeSpan.FromMinutes(jwtSettings.Expiry)
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

    public TimeSpan GetDefaultexpiry() => TimeSpan.FromMinutes(jwtSettings.Expiry);
}