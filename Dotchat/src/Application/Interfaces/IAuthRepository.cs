using DotchatServer.src.Core.Entities;
using DotchatServer.src.Core.Enums;

namespace DotchatServer.src.Application.Interfaces;

public interface IAuthRepository
{
    Task<RegisterErrorType> CreateUserAsync(ApplicationUser applicationUser);
}
