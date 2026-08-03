namespace DotchatShared.src.DTOs;

public record struct AccessTokenInfo(string AccessToken, TimeSpan Expiry);