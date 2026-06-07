using DotchatServer.src.Core.Interfaces;
namespace DotchatServer.src.Application.DTOs.TemplateModels;

/// <summary>
/// Represents the status of an email confirmation, including the application name, resend URL, and language.
/// </summary>
/// <param name="AppName">The name of the application.</param>
/// <param name="Language">The language of the email content.</param>
public sealed record EmailConfirmedModel(string AppName, string Language) : ITemplateNecessities;