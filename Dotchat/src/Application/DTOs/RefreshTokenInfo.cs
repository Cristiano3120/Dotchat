using DotchatShared.src.DTOs;
using DotchatShared.src.Enums;

namespace DotchatServer.src.Application.DTOs;

/// <summary>
/// Represents a single refresh token issued to one specific device for one user.
/// Relationship is 1:n (one user can have many tokens, one per device) to support
/// concurrent logins from multiple devices/sessions.
/// </summary>
public sealed class RefreshTokenInfo
{
    public Guid Id { get; private init; }

    public required Snowflake UserId { get; init; }

    public required Guid DeviceId { get; init; }

    /// <summary>
    /// Hash of the actual refresh token value. The raw token is never stored —
    /// only ever sent to the client once at issuance. If the database is ever
    /// leaked, an attacker only gets hashes, not usable tokens.
    /// </summary>
    public required byte[] TokenHash { get; init; }

    /// <summary>
    /// Optional display name for the "manage your devices" UI, e.g. "Cristiano's iPhone".
    /// Limited to 30 chars
    /// </summary>
    public string? DeviceName
    {
        get;
        private init
        {
            if (value?.Length > 30)
            {
                throw new ArgumentException($"{nameof(DeviceId)} is not allowed to have more than 30 chars");
            }

            field = value;
        }
    }

    /// <summary>Optional platform tag, e.g. "iOS", "Android", "Web".</summary>
    public required Platform Platform { get; init; }

    public DateTime CreatedAt { get; private init; }

    /// <summary>Natural expiry, set at issuance. Independent of <see cref="RevokedAt"/>.</summary>
    public DateTime ExpiresAt { get; private init; }

    /// <summary>Updated on every successful refresh; useful for showing "last active" and for cleanup.</summary>
    public DateTime LastUsedAt { get; set; }

    /// <summary>
    /// Timestamp of explicit invalidation (logout, password change, suspected theft,
    /// or rotation). Null means the token was never actively revoked — it may still
    /// be expired naturally, which is a separate, unrelated condition.
    ///
    /// This is kept distinct from deleting the row: a deleted row is indistinguishable
    /// from "never existed", while a revoked-but-still-used token is a strong signal
    /// of token theft (see reuse detection below) and is worth logging/alerting on.
    /// </summary>
    public DateTime? RevokedAt { get; set; }

    public bool IsRevoked => RevokedAt is not null;
    public bool IsExpired => ExpiresAt < DateTime.UtcNow;
    public bool IsValid => !IsRevoked && !IsExpired;

    public RefreshTokenInfo(TimeSpan expiry, string? deviceName = null)
    {
        Id = Guid.NewGuid();
        DeviceName = deviceName;

        DateTime currentTime = DateTime.UtcNow;
        ExpiresAt = currentTime + expiry;
        LastUsedAt = currentTime;
        CreatedAt = currentTime;
    }

    /// <summary>
    /// Parameterless ctor for efcore
    /// </summary>
    private RefreshTokenInfo() { }
}