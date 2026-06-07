using DotchatServer.src.Application.DTOs.TemplateModels;
using DotchatServer.src.Core.Entities;
using Microsoft.Extensions.Options;

namespace DotchatServer.src.Application.Factories;

/// <summary>
/// Factory class responsible for creating instances of <see cref="EmailConfirmationFailedModel"/> using application settings and provided parameters.
/// </summary>
/// <param name="options"></param>
internal sealed class EmailConfirmationFailedModelFactory(IOptions<AppSettings> options)
{
    private readonly AppSettings _settings = options.Value;

    /// <summary>
    /// Creates an EmailConfirmationFailedModel instance using the application name from settings, the provided resend URL, and language code.
    /// </summary>
    /// <param name="resendUrl">The URL to resend the email confirmation.</param>
    /// <param name="lang">The language code e.g., "en" for English.</param>
    /// <returns>An instance of <see cref="EmailConfirmationFailedModel"/>.</returns>
    public EmailConfirmationFailedModel CreateModel(string resendUrl, string lang) => new
    (
        AppName: _settings.AppName,
        ResendUrl: resendUrl,
        Language: lang
    );
}