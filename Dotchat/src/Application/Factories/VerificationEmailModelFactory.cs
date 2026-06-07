using DotchatServer.src.Application.DTOs.EmailModels;
using DotchatServer.src.Core.Config;
using DotchatServer.src.Core.Entities;
using DotchatShared.src.Constants;
using Microsoft.Extensions.Options;

namespace DotchatServer.src.Application.Factories;

/// <summary>
/// Factory for creating <see cref="VerificationEmailModel"/> instances.
/// Encapsulates configuration-based defaults like app name and expiration time.
/// </summary>
internal sealed class VerificationEmailFactory(IOptions<AppSettings> options, IOptions<ConfirmationEmailConfig> confirmationEmailConfig)
{
    private readonly ConfirmationEmailConfig _confirmationEmailConfig = confirmationEmailConfig.Value;
    private readonly AppSettings _settings = options.Value;

    /// <summary>
    /// Creates a model for the verification email template, which includes the user's name, app name, confirmation URL, expiry time, and language. 
    /// The confirmation URL is constructed using the web address from the app settings and the token provided as a parameter.
    /// </summary>
    /// <param name="displayName">The name of the user for whom the email is being sent.</param>
    /// <param name="language">The language code e.g., "en" for English.</param>
    /// <param name="token">The verification token to be included in the confirmation URL.</param>
    /// <returns>An instance of <see cref="VerificationEmailModel"/>.</returns>
    public VerificationEmailModel CreateModel(string displayName, string confirmationUrl, string language, string token) => new()
    {
        DisplayName = displayName,
        AppName = _settings.AppName,
        ConfirmUrl = confirmationUrl,
        Expiry = TimeSpan.FromMinutes(_confirmationEmailConfig.ConfirmationEmailExpiration),
        Language = language
    };
}