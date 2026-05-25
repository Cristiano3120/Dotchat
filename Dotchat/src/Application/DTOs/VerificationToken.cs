using System.Buffers.Binary;
using System.Runtime.CompilerServices;
using Microsoft.AspNetCore.WebUtilities;
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
    public static VerificationToken Empty => new();

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
            int GuidSize = Unsafe.SizeOf<Guid>();
            Span<byte> data = WebEncoders.Base64UrlDecode(token);
            
            if (data.Length != GuidSize + sizeof(long))
            {
                verificationToken = Empty;
                return false;
            }
            
            verificationToken = new VerificationToken
            {
                RandomIdentifier = new(data[..GuidSize]),
                UserId = BinaryPrimitives.ReadInt64LittleEndian(data[GuidSize..])
            };

            return true;
        }
        catch
        {
            verificationToken = Empty;
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
        Span<byte> bytes = stackalloc byte[Unsafe.SizeOf<Guid>() + sizeof(long)];
        _ = RandomIdentifier.TryWriteBytes(bytes); //Write the GUID into the span

        //Writes the user ID as little-endian bytes immediately following the GUID bytes.
        BinaryPrimitives.WriteInt64LittleEndian(bytes[16..], UserId);
        
        return WebEncoders.Base64UrlEncode(bytes);
    }
}