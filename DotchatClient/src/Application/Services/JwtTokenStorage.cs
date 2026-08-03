using DotchatClient.src.Application.Interfaces;

namespace DotchatClient.src.Application.Services;

internal sealed class JwtTokenStorage : IJwtTokenStorage
{
    /// <summary>
    /// Defines the timespan that when undershot triggers a AccessToken refresh
    /// </summary>
    private readonly TimeSpan _refreshThreshold = TimeSpan.FromMinutes(1);
    private DateTimeOffset _accessTokenExpiry;
    private string _accessToken = string.Empty;

    public void SetAccessToken(string token, DateTimeOffset expiry)
    {
        _accessTokenExpiry = expiry;
        _accessToken = token;
    }

    /// <summary>
    /// Returns the accessToken and an indicator whether or not a new one should be requested
    /// </summary>
    /// <returns></returns>
    public (string? token, bool requestNewToken) GetAccessToken()
    {
        //Token not set yet -> User not logged in
        if (_accessToken == string.Empty)
        {
            return (null, false);
        }

        //Token expired
        if (_accessTokenExpiry < DateTimeOffset.Now)
        {
            return (null, true);
        }

        TimeSpan leftoverTime = _accessTokenExpiry - DateTimeOffset.Now;
        if (leftoverTime < _refreshThreshold) //Token still active but close to expiring
        {
            return (_accessToken, true);
        }
        
        return (_accessToken, false);
    }

    public async Task<string?> GetRefreshTokenAsync() 
        => await SecureStorage.GetAsync(key: "refresh_token");

    public async Task SetRefreshTokenAsync(string token) 
        => await SecureStorage.SetAsync(key: "refresh_token", token);
}