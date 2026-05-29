using DotchatServer.src.Application.DTOs;
using DotchatServer.src.Core.Entities;
using Microsoft.Extensions.Options;

namespace DotchatServer.src.Application.Factories;

public sealed class ResendConfirmationEmailModelFactory(IOptions<AppSettings> options, IOptions<ConfirmationEmailConfig> emailConfig)
{
    private readonly ConfirmationEmailConfig _emailConfig = emailConfig.Value;
    private readonly AppSettings _settings = options.Value;

    public ResendConfirmationEmailModel CreateModel(ApplicationUser user, string resendUrl, string lang) => new
    (
        AppName: _settings.AppName,
        Name: user.DisplayName,
        ResendUrl: resendUrl,
        Language: lang,
        ExpiresAt: DateTimeOffset.UtcNow.AddMinutes(_emailConfig.ConfirmationEmailExpiration)
    );
}
