using DotchatServer.src.Core.Entities;

namespace DotchatServer.src.Application.Interfaces;

public interface IAuthRepository
{
    Task<bool> CreateUser(ApplicationUser applicationUser);
}
