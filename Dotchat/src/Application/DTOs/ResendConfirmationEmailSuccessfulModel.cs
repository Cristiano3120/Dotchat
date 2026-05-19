namespace DotchatServer.src.Application.DTOs;

public sealed record ResendConfirmationEmailSuccessfulModel(string AppName, string Name, string ResendUrl, DateTimeOffset ExpiresAt);