using System.ComponentModel.DataAnnotations;
using DotchatShared.src.Enums;
using RRR = DotchatShared.src.Constants.RegisterRequestRules;

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
    [Required][EmailAddress] string Email,
    [Required][MinLength(RRR.MinPasswordLength)][MaxLength(RRR.MaxPasswordLength)] string Password,
    [Required][MinLength(RRR.MinUsernameLength)][MaxLength(RRR.MaxUsernameLength)][RegularExpression(@"^[a-zA-Z0-9_.-]+$")] string Username,
    [Required][EnumDataType(typeof(Platform))] Platform? Platform,
    [Required] DateOnly? Birthday,
    [Required] Guid? DeviceId,
    [Required][MinLength(RRR.MinDisplayNameLength)][MaxLength(RRR.MaxDisplayNameLength)] string DisplayName,
    [MaxLength(RRR.MaxBioLength)] string Bio,
    [Required] string DeviceName
);