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

    /// <summary>
    /// A random identifier,  that is combined with the UserId to create a unique token. 
    /// This ensures that even if the same user requests multiple tokens, each token will be unique and can be invalidated independently.
    /// Additionally, the random identifier adds an extra layer of security by making the token harder to guess or forge, as it is not solely based on the user ID.
    /// </summary>
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


    /// <summary>
    /// Formats the token as a base64 string containing the random identifier and the user ID.
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        int GuidSize = Unsafe.SizeOf<Guid>();

        Span<byte> bytes = stackalloc byte[GuidSize + sizeof(long)];
        _ = RandomIdentifier.TryWriteBytes(bytes); //Write the GUID into the span

        //Writes the user ID as little-endian bytes immediately following the GUID bytes.
        BinaryPrimitives.WriteInt64LittleEndian(bytes[GuidSize..], UserId);
        
        return WebEncoders.Base64UrlEncode(bytes);
    }
    
    public static implicit operator string(VerificationToken token) => token.ToString();
    public static implicit operator RedisKey(VerificationToken token) => token.ToString();
}