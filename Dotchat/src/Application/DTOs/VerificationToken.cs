using System.Buffers.Binary;
using System.Runtime.CompilerServices;
using DotchatShared.src.DTOs;
using Microsoft.AspNetCore.WebUtilities;
using StackExchange.Redis;

namespace DotchatServer.src.Application.DTOs;

/// <summary>
/// Represents a compact verification token that combines a user identifier with a random Guid and can be serialized to
/// and from a base64 string.
/// </summary>
/// <remarks>The token is encoded as UTF-8 bytes of the Guid string followed immediately by the user ID string,
/// then base64-encoded. Create a new token with <see cref="VerificationToken.New(Snowflake)"/> which generates a fresh Guid;
/// Use <see cref="VerificationToken.TryParse(string, out VerificationToken)"/> to parse the string representation back to the struct.</remarks>
public readonly record struct VerificationToken
{
    public Snowflake UserId { get; init; }

    public Guid RandomIdentifier { get; init; }
    public static VerificationToken Empty => new();

    public static VerificationToken New(Snowflake userId) => new()
    {
        UserId = userId,
        RandomIdentifier = Guid.NewGuid()
    };

    /// <summary>
    /// Parses a Base64 URL-encoded verification token into a <see cref="VerificationToken"/> by decoding the token and extracting a
    /// GUID-sized random identifier followed by a little-endian 64-bit user identifier.
    /// </summary>
    /// <remarks>Decoding or format errors are caught and cause the method to return false with
    /// verificationToken set to VerificationToken.Empty.</remarks>
    /// <param name="token">Base64 URL-encoded token containing a GUID-sized random identifier followed by a 64-bit little-endian user
    /// identifier.</param>
    /// <param name="verificationToken">When true is returned, contains the parsed VerificationToken; otherwise set to VerificationToken.Empty.</param>
    /// <returns>True if the token was decoded and matched the expected format and length; otherwise false.</returns>
    public static bool TryParse(string token, out VerificationToken verificationToken)
    {
        try
        {
            int GuidSize = Unsafe.SizeOf<Guid>();
            Span<byte> data = WebEncoders.Base64UrlDecode(token);

            if (data.Length != GuidSize + Unsafe.SizeOf<Snowflake>())
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

    /// <summary>
    /// Formats the token as a base64 string containing the random identifier and the user ID.
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        int GuidSize = Unsafe.SizeOf<Guid>();

        Span<byte> bytes = stackalloc byte[GuidSize + Unsafe.SizeOf<Snowflake>()];
        _ = RandomIdentifier.TryWriteBytes(bytes); //Write the GUID into the span

        //Writes the user ID as little-endian bytes immediately following the GUID bytes.
        BinaryPrimitives.WriteInt64LittleEndian(bytes[GuidSize..], UserId);
        
        return WebEncoders.Base64UrlEncode(bytes);
    }

    /// <summary>
    /// Allows implicit conversion of a VerificationToken to a string by calling the ToString() method, which returns the base64-encoded representation of the token.
    /// </summary>
    public static implicit operator string(VerificationToken token) => token.ToString();
    /// <summary>
    /// Allows implicit conversion of a VerificationToken to a RedisKey which itself implicitly converts to a string by calling the ToString() method, which returns the base64-encoded representation of the token.
    /// </summary>
    public static implicit operator RedisKey(VerificationToken token) => token.ToString();
}