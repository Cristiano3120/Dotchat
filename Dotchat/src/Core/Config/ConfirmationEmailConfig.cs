namespace DotchatServer.src.Core.Config;

/// <summary>
/// Contains configuration settings for confirmation emails, such as the expiration time for confirmation links.
/// </summary>
public sealed record ConfirmationEmailConfig
{
    /// <summary>
    /// The expiration time for confirmation emails, in minutes.
    /// </summary>
    public required int ConfirmationEmailExpiration { get; init; }
}