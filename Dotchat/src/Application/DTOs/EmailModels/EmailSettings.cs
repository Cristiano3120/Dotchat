namespace DotchatServer.src.Application.DTOs.EmailModels;

public sealed record EmailOptions
{
    public string Host { get; init; } = default!;
    public int Port { get; init; }
    public string Username { get; init; } = default!;
    public string SenderName { get; init; } = default!;
    public string SenderEmail { get; init; } = default!;
}