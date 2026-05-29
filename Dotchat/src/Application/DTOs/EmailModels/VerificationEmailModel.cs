using DotchatServer.src.Application.Factories;
using DotchatServer.src.Core.Interfaces;

namespace DotchatServer.src.Application.DTOs.EmailModels;

/// <summary>
/// DONT CONSTRUCT YOURSELF. USE THE <see cref="VerificationEmailFactory"/>
/// </summary>
public sealed class VerificationEmailModel : ITemplateNecessities
{
    public required string Name { get; init; }
    public required string AppName { get; init; }
    public required string ConfirmUrl { get; init; }
    public required string Language { get; init; }
    public required TimeSpan Expiry { get; init; }

    /// <summary>
    /// Needed for the email template, but not actually used in the code. 
    /// We can calculate the expiration time on the fly using the Expiry property.
    /// </summary>
    public DateTime ExpiresAt => DateTime.UtcNow.Add(Expiry);

    internal VerificationEmailModel() { }
}