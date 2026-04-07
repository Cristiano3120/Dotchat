using DotchatServer.src.Application.DTOs;
using DotchatServer.src.Application.Enums;
using DotchatServer.src.Application.Interfaces;
using DotchatServer.src.Application.Interfaces.Security;
using DotchatServer.src.Core.Entities;
using DotchatShared.src.DTOs.AuthRequests;
using DotchatShared.src.Enums;
using System.Diagnostics;

namespace DotchatServer.src.Application.Services;

public sealed class AuthService(
    [FromKeyedServices(HashingAlgorithm.Argon2)] IHashingService hashingService,
    SnowflakeGenerator snowflakeGenerator,
    IAuthRepository authRepository,
    IJwtService jwtService)
{
    public async Task<RegisterResult> RegisterAsync(RegisterRequest registerRequest)
    {
        Stopwatch stopwatch = Stopwatch.StartNew();
        ApplicationUser applicationUser = new()
        {
            Id = snowflakeGenerator.NextId(),
            Username = registerRequest.Username,
            Birthday = registerRequest.Birthday,
            DisplayName = registerRequest.DisplayName,
            Email = registerRequest.Email,
            PasswordHash = hashingService.Hash(registerRequest.Password)
        };
        Console.WriteLine($"Hashing took: {stopwatch.ElapsedMilliseconds}ms");

        return await authRepository.CreateUserAsync(applicationUser) switch
        {
            RegisterErrorType.EmailTaken => new RegisterError(RegisterErrorType.EmailTaken),
            RegisterErrorType.UsernameTaken => new RegisterError(RegisterErrorType.UsernameTaken),
            RegisterErrorType.DbUnavailable => new RegisterError(RegisterErrorType.DbUnavailable),
            RegisterErrorType.Unknown => new RegisterError(RegisterErrorType.Unknown),
            RegisterErrorType.None => new RegisterResponse(jwtService.GenerateToken(userId: applicationUser.Id, email: applicationUser.Email)),
            _ => throw new NotImplementedException()
        };
    }
}
