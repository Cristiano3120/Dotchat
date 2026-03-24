using DotchatServer.src.Application.Enums;
using DotchatServer.src.Application.Interfaces;
using DotchatServer.src.Application.Interfaces.Security;
using DotchatServer.src.Core.Entities;
using DotchatShared.src.DTOs.AuthRequests;

namespace DotchatServer.src.Application.Services;

public sealed class AuthService(
    [FromKeyedServices(HashingAlgorithm.Argon2)] IHashingService hashingService,
    IAuthRepository authRepository)
{
    public async Task RegisterAsync(RegisterRequest registerRequest)
    {
        ApplicationUser applicationUser = new()
        {
            Id = 0, //TODO: Issue #11
            Username = registerRequest.Username,
            Birthday = registerRequest.Birthday,
            DisplayName = registerRequest.Display,
            Email = registerRequest.Email,
            PasswordHash = hashingService.Hash(registerRequest.Password)
        };
        await authRepository.CreateUser(applicationUser);
    }
}
