namespace DotchatServer.src.Application.DTOs.JwtModels;

/// <summary>
/// Represents the data related to JWT tokens for a client, including the refresh token, access token, and their expiry time.
/// </summary>
/// <param name="RefreshToken">The refresh token used to obtain a new access token when the current one expires.</param>
/// <param name="AccessToken">The access token used to authenticate requests to the server.</param>
/// <param name="Expiry">The time span after which the access token will expire.</param>
public sealed record JwtClientData(string RefreshToken, string AccessToken, TimeSpan Expiry);