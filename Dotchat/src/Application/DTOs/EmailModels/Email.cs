using DotchatServer.src.Application.Interfaces;

namespace DotchatServer.src.Application.DTOs.Emails;

public sealed record Email(string Subject, string HtmlBody) : IHtmlRenderable<Email>
{
    /// <summary>
    /// Enables implicit conversion of an Email object to a string by returning its HtmlBody property.
    /// </summary>
    public static implicit operator string(Email email) => email.HtmlBody;
}