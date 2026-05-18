using DotchatServer.src.Application.DTOs;
using DotchatServer.src.Application.DTOs.EmailModels;
using DotchatServer.src.Application.DTOs.Emails;
using DotchatServer.src.Application.DTOs.JwtModels;
using DotchatServer.src.Application.Enums;
using DotchatServer.src.Application.Factories;
using DotchatServer.src.Application.Interfaces;
using DotchatServer.src.Application.Interfaces.Security;
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
    EmailConfirmationStatusModelFactory emailConfirmationStatusModelFactory,
    VerificationEmailFactory verificationEmailFactory,
    SnowflakeGenerator snowflakeGenerator,
    [FromKeyedServices(HashingAlgorithm.Argon2)] IHashingService hashingService,
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
        };

        JwtClientData jwtData = jwtService.GenerateToken(userId: applicationUser.Id, email: applicationUser.Email);
        RefreshTokenInfo refreshTokenInfo = new(applicationUser.Id, hashingService.Hash(jwtData.RefreshToken), DateTimeOffset.UtcNow.Add(jwtData.Expiery));

        RegisterErrorType registrationResult = await authRepository.CompleteRegistrationAsync(applicationUser, refreshTokenInfo, registerRequest.Password);

        if (registrationResult != RegisterErrorType.None)
        {
            return new RegisterError(registrationResult);
        }

        await SendVerificationEmailAsync(applicationUser);
        return new RegisterResponse(jwtData);
    }

    private async Task SendVerificationEmailAsync(ApplicationUser applicationUser)
    {
        MailboxAddress to = new(name: applicationUser.DisplayName, address: applicationUser.Email);
        TimeSpan expiery = TimeSpan.FromMinutes(15);
        string token = await PrepVerificationAsync(applicationUser.Id, expiery);

        VerificationEmailModel verificationEmailModel =
            verificationEmailFactory.CreateModel(applicationUser.Username, Language.De, token, expiery);

        Email email = await emailFactory.CreateAsync<VerificationEmailModel>(
            templateName: Templates.EmailTemplates.VerificationEmail,
            model: verificationEmailModel
        );

        _ = await emailClient.TrySendEmailAsync(recipients: [to], email);
    }

    public async Task<string> ConfirmEmailAsync(string token, string language)
    {
        Log.Debug("Confirming email with token: {Token}", token);
        RedisValue userId = await redisConn.GetDatabase().StringGetAsync(token);

        string templateName = Templates.HtmlTemplates.EmailConfirmationFailed;
        EmailConfirmationStatus emailConfirmationStatus = emailConfirmationStatusModelFactory.CreateModel(
            userId: (long)userId,
            language: language
        );
        
        if (!userId.HasValue)
        {
            Log.Warning("Failed to confirm email with token: {Token}. Error: {Error}", token, "Invalid token");
            return await emailConfirmationTemplateFactory.CreateAsync<EmailConfirmationStatus>(templateName, emailConfirmationStatus);
        }

        Log.Debug("Token valid for user ID: {UserId}", userId);

        bool emailConfirmed = await authRepository.ConfirmEmailAsync((long)userId);
        if (emailConfirmed)
        {
            _ = await redisConn.GetDatabase().KeyDeleteAsync(token);
            templateName = Templates.HtmlTemplates.EmailConfirmed;
        }

        return await emailConfirmationTemplateFactory.CreateAsync<EmailConfirmationStatus>(templateName, emailConfirmationStatus);
    }

    private async Task<string> PrepVerificationAsync(long userID, TimeSpan expiery)
    {
        string token = Guid.NewGuid().ToString();
        _ = await redisConn.GetDatabase().StringSetAsync(key: token, value: userID, expiery);

        return token;
    }
}