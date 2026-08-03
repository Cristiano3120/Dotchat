using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using DotchatServer.src.Application.DTOs;
using DotchatServer.src.Application.Interfaces;
using DotchatShared.src.DTOs;
using Microsoft.IdentityModel.Tokens;

namespace DotchatServer.src.Application.Services;

internal sealed class JwtService(JwtSettings jwtSettings) : IJwtService
{
    public TimeSpan DefaultRefreshTokenExpiry => TimeSpan.FromDays(jwtSettings.RefreshTokenExpiry);
    private TimeSpan DefaultAccessTokenExpiry => TimeSpan.FromMinutes(jwtSettings.AccessTokenExpiry);

    public JwtClientData GenerateJwtClientData(Snowflake userId, Guid deviceId) 
        => new        
        (
            AccessTokenInfo: GenerateAccessToken(userId, deviceId),
            RefreshToken: GenerateRefreshToken()
        );


    public AccessTokenInfo GenerateAccessToken(Snowflake userId, Guid deviceId)
    {
        SymmetricSecurityKey key = new(key: Encoding.UTF8.GetBytes(jwtSettings.Key));

        Claim[] claims =
        [
            new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new Claim("device_id", deviceId.ToString()),
            new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(),ClaimValueTypes.Integer64),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        ];

        JwtSecurityToken token = new(
            issuer: jwtSettings.Issuer,
            audience: jwtSettings.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(jwtSettings.AccessTokenExpiry),
            signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256)
        );

        return new AccessTokenInfo(AccessToken: new JwtSecurityTokenHandler().WriteToken(token), Expiry: DefaultAccessTokenExpiry);
    }

    public string GenerateRefreshToken()
    {
        byte[] randomNumber = new byte[64];
        using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }
    }
}