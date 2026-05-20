using DotchatServer.src.Core.Interfaces;

namespace DotchatServer.src.Application.DTOs;

public sealed record ResendConfirmationEmailModel
(
    string AppName, 
    string Name, 
    string ResendUrl, 
    string Language,
    DateTimeOffset ExpiresAt) : ITemplateNecessities;