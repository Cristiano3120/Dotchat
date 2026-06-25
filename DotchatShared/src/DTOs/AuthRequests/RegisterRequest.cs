using System.ComponentModel.DataAnnotations;
using DotchatShared.src.Enums;

namespace DotchatShared.src.DTOs.AuthRequests;

/// <summary>
/// Represents the data required to register a new user account.
/// If this record is used within a ApiController and a Data Annotation fails it will automatically return: 400 BAD REQUEST
/// </summary>
/// <remarks>All fields are required to successfully register a new user. Validation attributes are applied to
/// ensure minimum requirements for email, password, and username are met.</remarks>
/// <param name="Email">The email address of the user. Must be a valid email format.</param>
/// <param name="Password">The password for the new account. Must be at least 8 characters in length.</param>
/// <param name="Username">The username for the new account. Must be at least 3 characters in length.</param>
/// <param name="DisplayName">The display name to associate with the user account.</param>
/// <param name="Birthday">The user's date of birth.</param>
public sealed record RegisterRequest(
    [Required][EmailAddress][MaxLength(254)] string Email,
    [Required][MinLength(8)][MaxLength(72)] string Password,
    [Required][MinLength(3)][MaxLength(32)][RegularExpression(@"^[a-zA-Z0-9_.-]+$")] string Username,
    [Required][EnumDataType(typeof(Platform))] Platform? Platform,
    [Required] DateTimeOffset? Birthday,
    [Required] Guid? DeviceId,
    [Required][MinLength(1)][MaxLength(64)] string DisplayName,
    [MaxLength(100)] string? DeviceName
);