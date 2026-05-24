using DotchatServer.src.Application.Interfaces;

namespace DotchatServer.src.Application.DTOs;

public sealed class RawHtmlTemplate(string htmlBody) : IHtmlRenderable
{
    public string HtmlBody { get; init; } = htmlBody;
}