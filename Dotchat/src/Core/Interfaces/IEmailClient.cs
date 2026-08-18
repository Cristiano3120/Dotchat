using DotchatServer.src.Application.DTOs.EmailModels;
using MimeKit;

namespace DotchatServer.src.Core.Interfaces;

internal interface IEmailClient
{
    Task<bool> TrySendEmailAsync(IEnumerable<MailboxAddress> recipients, EmailTemplate email);
}