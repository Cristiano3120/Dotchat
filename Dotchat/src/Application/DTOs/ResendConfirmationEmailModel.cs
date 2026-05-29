using DotchatServer.src.Core.Interfaces;

namespace DotchatServer.src.Application.DTOs;

/// <summary>
/// Represents the data necessary for resending a confirmation email to a user. 
/// This model includes the application name, user's name, URL for resending the confirmation email, 
/// preferred language, and the expiration time for the resend link.
/// </summary>
/// <param name="AppName">The name of the application.</param>
/// <param name="Name">The name of the user.</param>
/// <param name="ResendUrl">The URL for resending the confirmation email.</param>
/// <param name="Language">The preferred language of the user.</param>
/// <param name="ExpiresAt">The expiration time for the resend link.</param>
public sealed record ResendConfirmationEmailModel(string AppName, string Name, string ResendUrl, string Language,
    DateTimeOffset ExpiresAt) : ITemplateNecessities;