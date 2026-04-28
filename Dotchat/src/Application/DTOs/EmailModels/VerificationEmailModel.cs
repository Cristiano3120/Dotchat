using DotchatServer.src.Application.Factories;
using DotchatServer.src.Core.Interfaces;

namespace DotchatServer.src.Application.DTOs.EmailModels;

/// <summary>
/// DONT CONSTRUCT YOURSELF. USE THE <see cref="VerificationEmailFactory"/>
/// </summary>
public sealed class VerificationEmailModel : IEmailTemplateNecessities
{
    public required string Name { get; init; }
    public required string AppName { get; init; }
    public required string ConfirmUrl { get; init; }
    public string Language { get; init; }
    public TimeSpan Expiery { get; init; }

    /// <summary>
    /// Needed for the email template, but not actually used in the code. 
    /// We can calculate the expiration time on the fly using the Expiery property.
    /// </summary>
    public DateTime ExpiresAt => DateTime.UtcNow.Add(Expiery);

    internal VerificationEmailModel() { }
}