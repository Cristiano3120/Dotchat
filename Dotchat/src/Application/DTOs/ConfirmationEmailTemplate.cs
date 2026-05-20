namespace DotchatServer.src.Application.DTOs;

public sealed class ConfirmationEmailTemplate
{
    public string HtmlBody { get; init; }

    public static implicit operator string(ConfirmationEmailTemplate template) => template.HtmlBody;
}