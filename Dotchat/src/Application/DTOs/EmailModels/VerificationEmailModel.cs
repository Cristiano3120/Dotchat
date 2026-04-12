namespace DotchatServer.src.Application.DTOs.EmailModels;

public record VerificationEmailModel(
    string Name,
    string AppName,
    string ConfirmUrl,
    DateTime ExpiresAt,
    string Language = "de");