using DotchatServer.src.Application.Interfaces;

namespace DotchatServer.src.Application.DTOs.Emails;

public sealed record Email(string Subject, string HtmlBody) : IHtmlRenderable<Email>
{
    public static implicit operator string(Email email) => email.HtmlBody;
}