namespace DotchatClient.src.Application.Interfaces;

internal interface IJwtTokenStorage
{
    void SetAccessToken(string token, DateTimeOffset expiry);
    (string? token, bool requestNewToken) GetAccessToken();
    Task SetRefreshTokenAsync(string token);
    Task<string?> GetRefreshTokenAsync();
}