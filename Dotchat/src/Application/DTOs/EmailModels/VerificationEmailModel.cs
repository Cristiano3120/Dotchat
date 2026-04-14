using DotchatServer.src.Core.Interfaces;

namespace DotchatServer.src.Application.DTOs.EmailModels;

public record VerificationEmailModel(
    string Name,
    string AppName,
    string ConfirmUrl,
    DateTime ExpiresAt,
    string Subject,
    string Language = "de") : IEmailTemplateNecessities;