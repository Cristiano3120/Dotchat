using DotchatServer.src.Application.DTOs;
using DotchatServer.src.Application.Enums;
using DotchatServer.src.Application.Interfaces;
using DotchatServer.src.Application.Interfaces.Security;
using DotchatServer.src.Core.Entities;
using DotchatServer.src.Core.Enums;
using DotchatShared.src.DTOs.AuthRequests;

namespace DotchatServer.src.Application.Services;

public sealed class AuthService(
    [FromKeyedServices(HashingAlgorithm.Argon2)] IHashingService hashingService,
    IAuthRepository authRepository,
    SnowflakeGenerator snowflakeGenerator)
{
    public async Task<RegisterResult> RegisterAsync(RegisterRequest registerRequest)
    {
        ApplicationUser applicationUser = new()
        {
            Id = snowflakeGenerator.NextId(),
            Username = registerRequest.Username,
            Birthday = registerRequest.Birthday,
            DisplayName = registerRequest.Display,
            Email = registerRequest.Email,
            PasswordHash = hashingService.Hash(registerRequest.Password)
        };

        return await authRepository.CreateUserAsync(applicationUser) switch
        {
            RegisterErrorType.EmailTaken => new RegisterError(RegisterErrorType.EmailTaken, ""),
            RegisterErrorType.UsernameTaken => new RegisterError(RegisterErrorType.UsernameTaken, ""),
            RegisterErrorType.DbUnavailable => new RegisterError(RegisterErrorType.DbUnavailable, ""),
            RegisterErrorType.Unknown => new RegisterError(RegisterErrorType.Unknown, ""),
            RegisterErrorType.None => new RegisterResponse()
        };
    }
}
