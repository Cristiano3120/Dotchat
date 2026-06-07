namespace DotchatServer.src.Core.Config;

/// <summary>
/// Contains the necessary information to send emails, such as the SMTP host, port, username, sender name, and sender email address.
/// </summary>
public sealed record EmailConfig
{
    public required string Host { get; init; }
    public required int Port { get; init; }
    public required string Username { get; init; }
    public required string SenderName { get; init; }
    public required string SenderEmail { get; init; }

    /// <summary>
    /// Ctor for DI
    /// </summary>
    public EmailConfig() { }
}