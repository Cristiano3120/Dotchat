using DotchatServer.src.Application.Interfaces;

namespace DotchatServer.src.Application.DTOs;

/// <summary>
/// Represents an HTML template that contains raw HTML and implements IHtmlRenderable.
/// </summary>
public sealed record RawHtmlTemplate(string HtmlBody) : IHtmlRenderable;