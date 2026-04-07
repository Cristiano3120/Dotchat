using DotchatServer.src.Core.Entities;
using DotchatShared.src.Enums;

namespace DotchatServer.src.Application.Interfaces;

public interface IAuthRepository
{
    Task<RegisterErrorType> CreateUserAsync(ApplicationUser applicationUser);
}
