using Destructurama.Attributed;
using DotchatServer.src.Application.Services;

namespace DotchatServer.src.Core.Entities;

public sealed record ApplicationUser
{
    public long Id { get; init; }

    // ── PROFIL ────────────────────────────────────────────────────
    public string Email { get; init; } = string.Empty;
    public string Username {  get; init; } = string.Empty;
    public string DisplayName { get; init; } = string.Empty;
    public DateTimeOffset Birthday { get; init; }
    public string? Bio { get; init; }

    // ── Security ────────────────────────────────────────────────────
    [LogMasked]
    public string PasswordHash { get; init; } = string.Empty;
    public bool TwoFactorEnabled { get; init; } = false;
    public bool EmailVerified { get; init; } = false;

    // ── TIMESTAMPS ────────────────────────────────────────────────
    public DateTimeOffset CreatedAt => SnowflakeGenerator.GetCreationTime(Id);
}