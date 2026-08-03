namespace DotchatShared.src.DTOs;

/// <summary>
/// Represents the data related to JWT tokens for a client, including the refresh token, access token, and their expiry time.
/// </summary>
public readonly record struct JwtClientData(string RefreshToken, AccessTokenInfo AccessTokenInfo);