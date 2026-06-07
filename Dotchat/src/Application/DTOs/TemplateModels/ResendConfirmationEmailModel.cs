using DotchatServer.src.Core.Interfaces;

namespace DotchatServer.src.Application.DTOs.TemplateModels;

/// <summary>
/// Model for the email template used to resend the confirmation email to the user.
/// </summary>
public sealed record ResendConfirmationEmailModel(string AppName, string Name, string ResendUrl, string Language,
    DateTimeOffset ExpiresAt) : ITemplateNecessities;