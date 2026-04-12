using DotchatServer.src.Application.DTOs.Emails;
using DotchatServer.src.Core.Interfaces;

namespace DotchatServer.src.Infrastructure;

public sealed class EmailClient : IEmailClient
{
    public async Task<bool> TrySendEmailAsync(Email email)
    {
        throw new NotImplementedException();
    }
}