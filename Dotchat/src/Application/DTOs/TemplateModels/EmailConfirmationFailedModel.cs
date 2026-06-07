using DotchatServer.src.Core.Interfaces;

namespace DotchatServer.src.Application.DTOs.TemplateModels;

/// <summary>
/// Model for the email confirmation failure template
/// </summary>
public sealed record EmailConfirmationFailedModel(string AppName, string ResendUrl, string Language) : ITemplateNecessities;