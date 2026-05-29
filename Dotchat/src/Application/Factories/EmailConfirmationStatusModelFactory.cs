using DotchatServer.src.Application.DTOs;
using DotchatServer.src.Core.Entities;
using Microsoft.Extensions.Options;

namespace DotchatServer.src.Application.Factories;

/// <summary>
/// Creates an EmailConfirmationStatus model containing information about the email confirmation status, such as the app name,
/// </summary>
/// <param name="options">The application settings options which are obtained from the configuration.</param>
public sealed class EmailConfirmationStatusModelFactory(IOptions<AppSettings> options)
{
    private readonly AppSettings _settings = options.Value;

    public EmailConfirmedModel CreateModel(string lang) => new
    (
        AppName: _settings.AppName,
        Language: lang
    );
}