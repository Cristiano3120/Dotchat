using DotchatServer.src.Application.Factories;
using DotchatServer.src.Core.Interfaces;

namespace DotchatServer.src.Application.DTOs.EmailModels;

/// <summary>
/// DONT CONSTRUCT YOURSELF. USE THE <see cref="VerificationEmailFactory"/>
/// </summary>
public sealed class VerificationEmailModel : ITemplateNecessities
{
    public required string DisplayName { get; init; }
    public required string AppName { get; init; }

    /// <summary>
    /// The URL that the user will click to confirm their email address. 
    /// It is constructed using the web address from the app settings and the token provided as a parameter in the factory.
    /// </summary>
    public required string ConfirmUrl { get; init; }
    public required string Language { get; init; }

    /// <summary>
    /// TimeSpan in which the confirmation link will expire. 
    /// This is calculated based on the configuration for confirmation email expiration time, which is set in minutes.
    /// </summary>
    public required TimeSpan Expiry { get; init; }

    /// <summary>
    /// Needed for the email template, but not actually used in the code. 
    /// We can calculate the expiration time on the fly using the Expiry property.
    /// </summary>
    public DateTime ExpiresAt => DateTime.UtcNow.Add(Expiry);

    internal VerificationEmailModel() { }
}