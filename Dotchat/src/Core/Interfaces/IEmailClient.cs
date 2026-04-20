using DotchatServer.src.Application.DTOs.Emails;
using MimeKit;

namespace DotchatServer.src.Core.Interfaces;

public interface IEmailClient
{
    Task<bool> TrySendEmailAsync(IEnumerable<MailboxAddress> recipients, Email email);
}