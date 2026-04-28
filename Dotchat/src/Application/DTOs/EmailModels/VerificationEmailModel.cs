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
    public DateTime ExpiresAt => DateTime.UtcNow + TimeSpan.FromMinutes(15);

    internal VerificationEmailModel() { }
}