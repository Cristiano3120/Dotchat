using DotchatServer.src.Application.DTOs;
using DotchatServer.src.Core.Entities;
using DotchatShared.src.Enums;

namespace DotchatServer.src.Application.Interfaces;

public interface IAuthRepository
{
    Task<bool> ConfirmEmailAsync(long userId);
    Task<RegisterErrorType> CompleteRegistrationAsync(ApplicationUser applicationUser, RefreshTokenInfo refreshTokenInfo, string userPassword);
    Task<ApplicationUser> GetUserByIdAsync(long userID);
}