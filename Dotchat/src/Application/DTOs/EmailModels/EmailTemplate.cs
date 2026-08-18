using DotchatShared.src.Interfaces;

namespace DotchatServer.src.Application.DTOs.EmailModels;

public sealed record EmailTemplate(string Subject, string HtmlBody) : IHtmlRenderable<EmailTemplate>
{
    /// <summary>
    /// Enables implicit conversion of an Email object to a string by returning its HtmlBody property.
    /// </summary>
    public static implicit operator string(EmailTemplate email) => email.HtmlBody;
}