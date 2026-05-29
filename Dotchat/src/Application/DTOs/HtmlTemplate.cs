using DotchatServer.src.Application.Interfaces;

namespace DotchatServer.src.Application.DTOs;

/// <summary>
/// Represents an HTML template that can be rendered and returned as a response. This record encapsulates the HTML content
/// </summary>
/// <param name="HtmlBody">The HTML content of the template</param>
public sealed record HtmlTemplate(string HtmlBody) : IHtmlRenderable<HtmlTemplate>
{
    public static implicit operator string(HtmlTemplate value) => value.HtmlBody;
}