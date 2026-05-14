using DotchatServer.src.Application.DTOs;
using DotchatServer.src.Application.DTOs.EmailModels;
using DotchatServer.src.Application.DTOs.Emails;
using DotchatServer.src.Application.Factories;
using DotchatServer.src.Application.Interfaces;
using DotchatServer.src.Core.Entities;
using DotchatServer.src.Core.Interfaces;
using DotchatServer.src.Core.Templates;
using DotchatShared.src.DTOs.AuthRequests;
using DotchatShared.src.Enums;
using MimeKit;
using Serilog;
using StackExchange.Redis;

namespace DotchatServer.src.Application.Services;

public sealed class AuthService(
    VerificationEmailFactory verificationEmailFactory,
    SnowflakeGenerator snowflakeGenerator,
    ITemplateFactory<string> emailConfirmationTemplateFactory, 
    ITemplateFactory<Email> emailFactory,
    IConnectionMultiplexer redisConn,
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
            RegisterErrorType.None => await CompleteRegistrationAsync(applicationUser),
            _ => throw new NotImplementedException()
        };
    }

    public async Task<string> ConfirmEmailAsync(string token)
    {
        Log.Debug("Confirming email with token: {Token}", token);
        _ = await redisConn.GetDatabase().StringGetAsync(token).ContinueWith(async task =>
        {
            if (task.IsCompletedSuccessfully)
            {
                long userId = (long)task.Result;
                Log.Debug("Token valid for user ID: {UserId}", userId);

                bool emailConfirmed = await authRepository.ConfirmEmailAsync(userId);
                if (emailConfirmed)
                {
                    //return html page with success message
                    _ = await redisConn.GetDatabase().KeyDeleteAsync(token);
                }
                else
                {
                    //return html page with error message
                }
            }
            else
            {
                Log.Warning("Failed to confirm email with token: {Token}. Error: {Error}", token, task.Exception?.Message);
            }
        });

        EmailConfirmationStatusDto statusDTO = new EmailConfirmationStatusDto()
        {
            AppName = "Dotchat",
            Name = "User", //TODO Usernamen aus DB holen
            IsAlreadyConfirmed = false, //TODO aus DB hole
            LoginUrl = "https://dotchat.app/login",
            ResendUrl = "https://dotchat.app/resend-confirmation"
        };
        return await emailConfirmationTemplateFactory.CreateAsync<EmailConfirmationStatusDto>("EmailConfirmed", statusDTO, Language.De);
    }

    private async Task<RegisterResponse> CompleteRegistrationAsync(ApplicationUser applicationUser)
    {
        MailboxAddress to = new(name: applicationUser.DisplayName, address: applicationUser.Email);
        TimeSpan expiery = TimeSpan.FromMinutes(15);
        string token = await PrepVerificationAsync(applicationUser.Id, expiery);

        VerificationEmailModel verificationEmailModel =
            verificationEmailFactory.CreateModel(applicationUser.Username, Language.De, token, expiery);


        Email email = await emailFactory.CreateAsync<VerificationEmailModel>(
            templateName: Templates.EmailTemplates.VerificationEmail,
            model: verificationEmailModel,
            language: Language.De
        );

        _ = await emailClient.TrySendEmailAsync(recipients: [to], email); //TODO JWT In database speichern
        return new RegisterResponse(jwtService.GenerateToken(userId: applicationUser.Id, email: applicationUser.Email));
    }

    private async Task<string> PrepVerificationAsync(long userID, TimeSpan expiery)
    {
        string token = Guid.NewGuid().ToString();
        _ = await redisConn.GetDatabase().StringSetAsync(key: token, value: userID, expiery);

        return token;
    }
}