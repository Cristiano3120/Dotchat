namespace DotchatServer.src.Application.DTOs;

public sealed class ConfirmationEmailConfig
{
    public int ConfirmationEmailExpiration { get; init; } // in minutes
}