using System.ComponentModel.DataAnnotations;
using DotchatShared.src.Enums;

namespace DotchatShared.src.DTOs.AuthRequests;

/// <summary>
/// Represents the data required to request user authentication using an email address and password.
/// If this record is used within a ApiController and a Data Annotation fails it will automatically return: 400 BAD REQUEST
/// </summary>
/// <param name="Email">The email address associated with the user account. Must be a valid email format.</param>
/// <param name="Password">The password for the user account. Must be at least 8 characters in length.</param>
public sealed record LoginRequest(
    [Required][EmailAddress][MaxLength(254)] string Email,
    [Required][MinLength(8)][MaxLength(72)] string Password,
    [Required][EnumDataType(typeof(Platform))] Platform? Platform,
    [Required] Guid? DeviceId,
    [MaxLength(100)] string? DeviceName
);