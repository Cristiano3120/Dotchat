using DotchatServer.src.Application.DTOs;
using DotchatServer.src.Core.Entities;
using Microsoft.Extensions.Options;

namespace DotchatServer.src.Application.Factories;

/// <summary>
/// Do not construct this class directly. Use DI to obtain an instance of EmailConfirmationFailedModelFactory, then call CreateModel() with the appropriate parameters to get an EmailConfirmationFailedModel instance.
/// Creates an EmailConfirmationFailedModel instance using application settings and provided parameters.
/// </summary>
/// <param name="options">The application settings options. Obtained from DI.</param>
public sealed class EmailConfirmationFailedModelFactory(IOptions<AppSettings> options)
{
    private readonly AppSettings _settings = options.Value;

    public EmailConfirmationFailedModel CreateModel(string resendUrl, string lang) => new
    (
        AppName: _settings.AppName,
        ResendUrl: resendUrl,
        Language: lang
    );
}