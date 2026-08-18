namespace DotchatClient.src.Core.DTOs;

public sealed record OAuthCallbackModel
{
    public required bool Success { get; init; }
    public required string Title { get; init; }
    public required string Message { get; init; }
    public required string LogoDataUri { get; init; }
}