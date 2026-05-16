using DotchatServer.src.Core.Interfaces;
namespace DotchatServer.src.Application.DTOs;

public sealed record EmailConfirmationStatus : ITemplateNecessities
{
    public string AppName { get; set; } = string.Empty;
    public string ResendUrl { get; set; } = string.Empty;
    public string Language { get; set; } = DotchatShared.src.Enums.Language.En.ToString();
}
