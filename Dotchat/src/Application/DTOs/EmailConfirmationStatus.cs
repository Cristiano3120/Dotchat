using DotchatServer.src.Core.Interfaces;

namespace DotchatServer.src.Application.DTOs;

public class EmailConfirmationStatus : ITemplateNecessities
{
    public string AppName { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public bool IsAlreadyConfirmed { get; set; }
    public string LoginUrl { get; set; } = string.Empty;
    public string ResendUrl { get; set; } = string.Empty;
    public string Language { get; set; } = "De";
}
