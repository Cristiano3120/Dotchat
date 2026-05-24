using DotchatServer.src.Application.Interfaces;

namespace DotchatServer.src.Application.DTOs;

public sealed record ResendConfirmationEmailTemplate : IHtmlRenderable<ResendConfirmationEmailTemplate>
{
    public string HtmlBody { get; init; } = string.Empty;

    public static implicit operator string(ResendConfirmationEmailTemplate template) => template.HtmlBody;
}