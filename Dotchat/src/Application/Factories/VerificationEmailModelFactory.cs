using DotchatServer.src.Application.DTOs;
using DotchatServer.src.Application.DTOs.EmailModels;
using DotchatServer.src.Core.Entities;
using DotchatShared.src.Constants;
using Microsoft.Extensions.Options;

namespace DotchatServer.src.Application.Factories;

public sealed class VerificationEmailFactory(IOptions<AppSettings> options, IOptions<ConfirmationEmailConfig> confirmationEmailConfig)
{
    private readonly ConfirmationEmailConfig _confirmationEmailConfig = confirmationEmailConfig.Value;
    private readonly AppSettings _settings = options.Value;

    public VerificationEmailModel CreateModel(string name, string language, string token) => new()
    {
        Name = name,
        AppName = _settings.AppName,
        ConfirmUrl = $"{_settings.WebAddress}/{Endpoints.AuthEndpoints.ConfirmEmailEndpoint}?token={token}",
        expiry = TimeSpan.FromMinutes(_confirmationEmailConfig.ConfirmationEmailExpiration),
        Language = language
    };
}