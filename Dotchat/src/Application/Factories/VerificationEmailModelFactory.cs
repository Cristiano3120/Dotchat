using DotchatServer.src.Application.DTOs.EmailModels;
using DotchatServer.src.Core.Entities;
using DotchatShared.src.Constants;
using Microsoft.Extensions.Options;

namespace DotchatServer.src.Application.Factories;

public sealed class VerificationEmailFactory(IOptions<AppSettings> options)
{
    private readonly AppSettings _settings = options.Value;

    public VerificationEmailModel CreateModel(string name, string language, string token, TimeSpan expiery) => new()
    {
        Name = name,
        AppName = _settings.AppName,
        ConfirmUrl = $"{options.Value.WebAddress}/{Endpoints.AuthEndpoints.ConfirmEmailEndpoint}?token={token}",
        Expiery = expiery,
        Language = language
    };
}