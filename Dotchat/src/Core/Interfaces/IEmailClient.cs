using DotchatServer.src.Application.DTOs.Emails;

namespace DotchatServer.src.Core.Interfaces;

public interface IEmailClient
{
    Task<bool> TrySendEmailAsync(Email email);
}