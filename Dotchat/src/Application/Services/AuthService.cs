using DotchatServer.src.Application.DTOs;
using DotchatServer.src.Application.DTOs.EmailModels;
using DotchatServer.src.Application.DTOs.Emails;
using DotchatServer.src.Application.Interfaces;
using DotchatServer.src.Core.Entities;
using DotchatServer.src.Core.Interfaces;
using DotchatServer.src.Core.Templates;
using DotchatShared.src.DTOs.AuthRequests;
using DotchatShared.src.Enums;
using MimeKit;

namespace DotchatServer.src.Application.Services;

public sealed class AuthService(
    VerificationEmailFactory verificationEmailFactory,
    SnowflakeGenerator snowflakeGenerator,
    IAuthRepository authRepository,
    IEmailFactory emailFactory,
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
            RegisterErrorType.None => await CompleteRegistrationAsync(applicationUser),
            _ => throw new NotImplementedException()
        };
    }

    private async Task<RegisterResponse> CompleteRegistrationAsync(ApplicationUser applicationUser)
    {
        string token = await PrepVerificationAsync(applicationUser.Id);
        MailboxAddress to = new(name: applicationUser.DisplayName, address: applicationUser.Email);
        Email email = await emailFactory.CreateAsync<VerificationEmailModel>(
            templateName: Templates.EmailTemplates.VerificationEmail,
            model: verificationEmailFactory.CreateModel(applicationUser.Username, Language.De, token: token),
            language: Language.De
        );

        _ = await emailClient.TrySendEmailAsync(recipients: [to], email);
        return new RegisterResponse(jwtService.GenerateToken(userId: applicationUser.Id, email: applicationUser.Email));
    }

    //TODO HIER WEITER MACHEN: Token in DB speichern, damit wir ihn später überprüfen können. Außerdem Ablaufzeit festlegen
    //TODO: SUBJECT IN TEMPLATE PACKEN
    private async Task<string> PrepVerificationAsync(long userID)
    {
        string token = Guid.NewGuid().ToString();
        // Additional logic for preparing verification (e.g., saving token to database)
        return token;
    }

    public async Task VerifyAsync(long userID, string token)
    {

    }
}