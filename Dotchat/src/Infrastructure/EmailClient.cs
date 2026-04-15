using DotchatServer.src.Application.DTOs.Emails;
using DotchatServer.src.Core.Interfaces;

namespace DotchatServer.src.Infrastructure;

public sealed class EmailClient : IEmailClient
{
    public async Task<bool> TrySendEmailAsync(Email email)
    {
        //TODO: Use Mailkit //use dann die ui über http://localhost:8025 um die emails zu sehen ohne sie tatsächlich zu versenden
        throw new NotImplementedException();
    }
}