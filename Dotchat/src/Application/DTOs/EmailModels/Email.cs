using DotchatServer.src.Application.Interfaces;

namespace DotchatServer.src.Application.DTOs.Emails;

/// <summary>
/// Represents an email. Contains a subject and a HTML body
/// </summary>
public sealed record Email(string Subject, string HtmlBody) : IHtmlRenderable<Email>
{
    public static implicit operator string(Email email) => email.HtmlBody;
}