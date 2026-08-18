using DotchatShared.src.Interfaces;

namespace DotchatShared.src.DTOs;

/// <summary>
/// Represents an HTML template that contains raw HTML and implements IHtmlRenderable.
/// </summary>
public sealed record RawHtmlTemplate(string HtmlBody) : IHtmlRenderable;