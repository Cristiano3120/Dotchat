using DotchatServer.src.Application.DTOs;
using DotchatServer.src.Core.Entities;
using DotchatShared.src.Constants;
using Microsoft.Extensions.Options;

namespace DotchatServer.src.Application.Factories;

public sealed class EmailConfirmationStatusModelFactory(IOptions<AppSettings> options)
{
    private readonly AppSettings _settings = options.Value;

    public EmailConfirmationStatus CreateModel(long userId, string language) => new()
    {
        AppName = _settings.AppName,
        ResendUrl = $"{_settings.WebAddress}/{Endpoints.AuthEndpoints.ResendConfirmationEndpoint}?userId={userId}",
        Language = language
    };
}