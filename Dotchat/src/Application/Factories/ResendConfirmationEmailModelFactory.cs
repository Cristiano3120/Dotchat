using DotchatServer.src.Application.DTOs.TemplateModels;
using DotchatServer.src.Core.Config;
using DotchatServer.src.Core.Entities;
using Microsoft.Extensions.Options;

namespace DotchatServer.src.Application.Factories;

/// <summary>
/// Factory for creating <see cref="ResendConfirmationEmailModel"/> instances.
/// Encapsulates configuration-based defaults like app name and expiration time.
/// </summary>
internal sealed class ResendConfirmationEmailModelFactory(IOptions<AppSettings> options, IOptions<ConfirmationEmailConfig> emailConfig)
{
    private readonly ConfirmationEmailConfig _emailConfig = emailConfig.Value;
    private readonly AppSettings _settings = options.Value;

    /// <summary>
    /// Creates a model for the resend confirmation email template.
    /// </summary>
    /// <param name="displayName">The display name of the user for whom the email is being sent.</param>
    /// <param name="resendUrl">The URL to resend the email confirmation.</param>
    /// <param name="lang">The language code e.g., "en" for English.</param>
    /// <returns>An instance of <see cref="ResendConfirmationEmailModel"/>.</returns>
    public ResendConfirmationEmailModel CreateModel(string displayName, string resendUrl, string lang) => new
    (
        AppName: _settings.AppName,
        Name: displayName,
        ResendUrl: resendUrl,
        Language: lang,
        ExpiresAt: DateTimeOffset.UtcNow.AddMinutes(_emailConfig.ConfirmationEmailExpiration)
    );
}