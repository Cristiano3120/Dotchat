using DotchatServer.src.Application.Interfaces;

namespace DotchatServer.src.Application.DTOs;

public sealed class ConfirmationEmailTemplate : IHtmlRenderable<ConfirmationEmailTemplate>
{
    public string HtmlBody { get; init; } = string.Empty;
    public static implicit operator string(ConfirmationEmailTemplate template) => template.HtmlBody;
}