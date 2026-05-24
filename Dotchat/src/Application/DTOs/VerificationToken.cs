using System.Text;
using StackExchange.Redis;

namespace DotchatServer.src.Application.DTOs;

/// <summary>
/// Represents a compact verification token that combines a user identifier with a random Guid and can be serialized to
/// and from a base64 string.
/// </summary>
/// <remarks>The token is encoded as UTF-8 bytes of the Guid string followed immediately by the user ID string,
/// then base64-encoded. Create a new token with <see cref="VerificationToken.New(long)"/> which generates a fresh Guid;
/// Use <see cref="VerificationToken.TryParse(string, out DotchatServer.src.Application.DTOs.VerificationToken)"/> to parse the string representation back to the struct.</remarks>
public readonly record struct VerificationToken
{
    public long UserId { get; init; }
    public Guid RandomIdentifier { get; init; }

    /// <summary>
    /// Creates a new VerificationToken containing the UserID and <see cref="Guid.NewGuid"/> as the random identifier.
    /// </summary>
    /// <param name="userId"></param>
    /// <returns></returns>
    public static VerificationToken New(long userId) => new()
    {
        UserId = userId,
        RandomIdentifier = Guid.NewGuid()
    };

    public static bool TryParse(string token, out VerificationToken verificationToken)
    {
        try
        {
            byte[] data = Convert.FromBase64String(token);
            string result = Encoding.UTF8.GetString(data);

            verificationToken = new VerificationToken
            {
                RandomIdentifier = Guid.Parse(result[..Guid.NewGuid().ToString().Length]),
                UserId = long.Parse(result[Guid.NewGuid().ToString().Length..])
            };

            return true;
        }
        catch
        {
            verificationToken = default;
            return false;
        }
    }

    public static implicit operator string(VerificationToken token) => token.ToString();
    public static implicit operator RedisKey(VerificationToken token) => token.ToString();

    /// <summary>
    /// Formats the token as a base64 string containing the random identifier and the user ID.
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        byte[] data = Encoding.UTF8.GetBytes(RandomIdentifier.ToString() + UserId.ToString());
        return Convert.ToBase64String(data);
    }
}