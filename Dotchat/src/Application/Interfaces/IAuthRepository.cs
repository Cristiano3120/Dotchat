using DotchatServer.src.Application.DTOs;
using DotchatServer.src.Application.Enums;
using DotchatServer.src.Core.Entities;
using DotchatServer.src.Infrastructure;
using DotchatShared.src.DTOs;
using DotchatShared.src.Enums;

namespace DotchatServer.src.Application.Interfaces;

/// <summary>
/// Represents a repository interface for authentication-related operations, such as confirming email addresses,
/// completing user registration, and retrieving user information.
/// </summary>
public interface IAuthRepository
{
    Task<ApplicationUser?> FindUserByEmailAsync(string email);
    Task UpsertRefreshTokenAsync(RefreshTokenInfo refreshTokenInfo);
    Task<bool> CheckIfEmailExistsAsync(string email);
    Task<bool> CheckIfUsernameExistsAsync(string username);
    Task RegisterUserAsync(ApplicationUser user, RefreshTokenInfo tokenInfo);
    Task<bool> ConfirmEmailAsync(Snowflake userId);
    Task<ApplicationUser?> GetUserByIdAsync(Snowflake userID);
}