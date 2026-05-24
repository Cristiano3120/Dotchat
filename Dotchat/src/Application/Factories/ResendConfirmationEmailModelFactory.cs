using DotchatServer.src.Application.DTOs;
using DotchatServer.src.Core.Entities;
using DotchatShared.src.Constants;
using Microsoft.Extensions.Options;

namespace DotchatServer.src.Application.Factories;

public sealed class ResendConfirmationEmailModelFactory(IOptions<AppSettings> options)
{
    private readonly AppSettings _settings = options.Value;

    public ResendConfirmationEmailModel CreateModel(ApplicationUser user, string language, int expiry) => new
    (
        AppName: _settings.AppName,
        Name: user.DisplayName,
        ResendUrl: $"{_settings.WebAddress}/{Endpoints.AuthEndpoints.ResendConfirmationEndpoint}?userId={user.Id}",
        Language: language,
        ExpiresAt: DateTimeOffset.UtcNow.AddMinutes(expiry)
    );
}
