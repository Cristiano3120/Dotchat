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
    public string PasswordHash { get; init; } = string.Empty;

    // ── TIMESTAMPS ────────────────────────────────────────────────
    public DateTimeOffset CreatedAt => throw new NotImplementedException("Get Created from ID >>"); //TODO: Issue #12
}