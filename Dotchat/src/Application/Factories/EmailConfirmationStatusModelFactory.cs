using DotchatServer.src.Application.DTOs.TemplateModels;
using DotchatServer.src.Core.Entities;
using Microsoft.Extensions.Options;

namespace DotchatServer.src.Application.Factories;

/// <summary>
/// Creates an EmailConfirmationStatus model containing information about the email confirmation status
/// </summary>
/// <param name="options">The application settings options which are obtained from the configuration.</param>
internal sealed class EmailConfirmationStatusModelFactory(IOptions<AppSettings> options)
{
    private readonly AppSettings _settings = options.Value;

    /// <summary>
    /// Creates an EmailConfirmedModel containing the app name and the language for the email confirmation status email template.
    /// </summary>
    /// <param name="lang">The language code e.g., "en" for English.</param>
    /// <returns>An instance of <see cref="EmailConfirmedModel"/>.</returns>
    public EmailConfirmedModel CreateModel(string lang) => new
    (
        AppName: _settings.AppName,
        Language: lang
    );
}