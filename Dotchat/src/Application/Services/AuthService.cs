using DotchatServer.src.Application.DTOs;
using DotchatServer.src.Application.Interfaces;
using DotchatServer.src.Core.Entities;
using DotchatServer.src.Core.Interfaces;

using DotchatShared.src.DTOs.AuthRequests;
using DotchatShared.src.Enums;

namespace DotchatServer.src.Application.Services;

public sealed class AuthService(
    SnowflakeGenerator snowflakeGenerator,
    IAuthRepository authRepository,
    IEmailClient emailClient,
    IJwtService jwtService)
{
    public async Task<RegisterResult> RegisterAsync(RegisterRequest registerRequest)
    {
        //Dont hash password yet, we will do it in the repository.
        //Doing it here would be expensive if the email or username is already taken
        ApplicationUser applicationUser = new()
        {
            Id = snowflakeGenerator.NextId(),
            Username = registerRequest.Username,
            Birthday = registerRequest.Birthday,
            DisplayName = registerRequest.DisplayName,
            Email = registerRequest.Email,
            PasswordHash = registerRequest.Password
        };

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

    public async Task RequestVerificationAsync(int userID)
    {
        string token = Guid.NewGuid().ToString();
        //Email Factory RazorEngineCore
    }

    public async Task VerifyAsync(int userID, string token)
    {

    }
}