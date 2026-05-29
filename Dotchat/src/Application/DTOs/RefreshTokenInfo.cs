namespace DotchatServer.src.Application.DTOs;

/// <summary>
/// Contains information about a refresh token, including its unique identifier, associated user ID, hashed token value, expiration time, and creation time.
/// </summary>
public sealed record RefreshTokenInfo
{
    public Guid Id { get; private set; }
    public long UserId { get; init; }
    public byte[] TokenHash { get; init; }
    public DateTimeOffset ExpiresAt { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }

    public RefreshTokenInfo(long userId, byte[] tokenHash, DateTimeOffset expiresAt)
    {
        Id = Guid.NewGuid();
        UserId = userId;
        TokenHash = tokenHash;
        CreatedAt = DateTimeOffset.UtcNow;
        ExpiresAt = expiresAt;
    }

    // Parameterless constructor for EF Core
    private RefreshTokenInfo() { }
}