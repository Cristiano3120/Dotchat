namespace DotchatServer.src.Application.DTOs.EmailModels;

/// <summary>
/// Contains the necessary information to send emails, such as the SMTP host, port, username, sender name, and sender email address.
/// </summary>
public sealed record EmailOptions
{
    public required string Host { get; init; }
    public required int Port { get; init; }
    public required string Username { get; init; }
    public required string SenderName { get; init; }
    public required string SenderEmail { get; init; }

    private EmailOptions() { }
}