using DotchatServer.src.Core.Interfaces;

namespace DotchatServer.src.Application.DTOs;

/// <summary>
/// Represents the data required to render the email confirmation failure template
/// </summary>
/// <param name="AppName">The name of the application</param>
/// <param name="ResendUrl">The URL to resend the confirmation email</param>
/// <param name="Language">The language of the email</param>
public sealed record EmailConfirmationFailedModel(string AppName, string ResendUrl, string Language) : ITemplateNecessities;