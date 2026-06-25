using Destructurama.Attributed;

using DotchatServer.src.Application.Services;
using DotchatShared.src.DTOs;

namespace DotchatServer.src.Core.Entities;

public sealed record ApplicationUser
{
    public required Snowflake Id { get; init; }

    // ── PROFIL ────────────────────────────────────────────────────
    public required string Email { get; init; } = string.Empty;
    public required string Username { get; init; } = string.Empty;
    public required string DisplayName { get; init; } = string.Empty;
    public required DateTimeOffset Birthday { get; init; }
    public string? Bio { get; init; }

    // ── Security ────────────────────────────────────────────────────
    [LogMasked]
    public required byte[] PasswordHash { get; init; } = [];
    public bool TwoFactorEnabled { get; init; } = false;
    public bool EmailConfirmed { get; init; } = false;

    // ── TIMESTAMPS ────────────────────────────────────────────────
    public DateTimeOffset CreatedAt => SnowflakeGenerator.GetCreationTime(Id);
}