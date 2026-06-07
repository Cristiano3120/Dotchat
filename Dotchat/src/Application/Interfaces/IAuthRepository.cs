using DotchatServer.src.Application.DTOs;
using DotchatServer.src.Core.Entities;
using DotchatServer.src.Infrastructure;
using DotchatShared.src.Enums;

namespace DotchatServer.src.Application.Interfaces;

/// <summary>
/// Represents a repository interface for authentication-related operations, such as confirming email addresses,
/// completing user registration, and retrieving user information.
/// </summary>
public interface IAuthRepository
{
    /// <summary>
    /// Confirms the email by setting the EmailConfirmed property to true for the user with the specified userId. 
    /// </summary>
    /// <returns>A <see cref="Result{T}"/> indicating the result of the operation. 
    /// If the user was found but the email was already confirmed, the result will contain a value.</returns>
    Task<Result<bool>> ConfirmEmailAsync(long userId);

    /// <summary>
    /// Completes the registration process for a user by creating a new user record and associating it with the provided refresh token information.
    /// </summary>
    /// <returns>A <see cref="RegisterErrorType"/> indicating the result of the registration process.</returns>
    Task<RegisterErrorType> CompleteRegistrationAsync(ApplicationUser applicationUser, RefreshTokenInfo refreshTokenInfo, string userPassword);
    Task<ApplicationUser?> GetUserByIdAsync(long userID);
}