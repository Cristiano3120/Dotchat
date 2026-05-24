using System.Text;
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
using Microsoft.Extensions.Options;
using MimeKit;
using Serilog;
using StackExchange.Redis;

namespace DotchatServer.src.Application.Services;

public sealed class AuthService(
    ResendConfirmationEmailModelFactory resendConfirmationEmailModelFactory,
    EmailConfirmationStatusModelFactory emailConfirmationStatusModelFactory,
    VerificationEmailFactory verificationEmailFactory,
    SnowflakeGenerator snowflakeGenerator,
    IOptions<ConfirmationEmailConfig> confirmationEmailConfig,
    [FromKeyedServices(HashingAlgorithm.Argon2)] IHashingService hashingService,
    ITemplateFactory<ResendConfirmationEmailTemplate> resendConfirmationEmailTemplateFactory,
    ITemplateFactory<ConfirmationEmailTemplate> confirmationEmailTemplateFactory,
    ITemplateFactory<Email> emailFactory,
    IConnectionMultiplexer redisConn,
    IAuthRepository authRepository,
    IEmailClient emailClient,
    IJwtService jwtService)
{
    public async Task<RegisterResult> RegisterAsync(RegisterRequest registerRequest, string language)
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

        Log.Debug("Attempting to register user: {applicationUser}", applicationUser);

        JwtClientData jwtData = jwtService.GenerateToken(userId: applicationUser.Id, email: applicationUser.Email);
        RefreshTokenInfo refreshTokenInfo = new(applicationUser.Id, hashingService.Hash(jwtData.RefreshToken), DateTimeOffset.UtcNow.Add(jwtData.expiry));

        RegisterErrorType registrationResult = await authRepository.CompleteRegistrationAsync(applicationUser, refreshTokenInfo, registerRequest.Password);

        if (registrationResult != RegisterErrorType.None)
        {
            return new RegisterError(registrationResult);
        }

        await SendVerificationEmailAsync(applicationUser, language);
        return new RegisterResponse(jwtData);
    }

    public async Task<string> ResendVerificationEmailAsync(long userID, string language)
    {
        ApplicationUser applicationUser = await authRepository.GetUserByIdAsync(userID);
        if (applicationUser is null)
        {   //No need to return a template since a normal user can´t hit this path anyway
            return "User not found. The link you clicked or the request you made contained a faulty userID";
        }

        if (applicationUser.EmailConfirmed)
        {
            EmailConfirmationStatus emailConfirmationStatus = emailConfirmationStatusModelFactory.CreateModel(userId: applicationUser.Id, language);
            string templateName = Templates.HtmlTemplates.EmailConfirmed;

            return await confirmationEmailTemplateFactory.CreateAsync<EmailConfirmationStatus>(templateName, emailConfirmationStatus);
        }

        await SendVerificationEmailAsync(applicationUser, language); //Maybe mach das das nen bool returned
                                                                     
        ResendConfirmationEmailModel model = resendConfirmationEmailModelFactory.CreateModel(applicationUser, language, confirmationEmailConfig.Value.ConfirmationEmailExpiration);
        return await resendConfirmationEmailTemplateFactory.CreateAsync<ResendConfirmationEmailModel>(Templates.HtmlTemplates.ResendConfirmation, model);
    }

    private async Task SendVerificationEmailAsync(ApplicationUser applicationUser, string language)
    {
        MailboxAddress to = new(name: applicationUser.DisplayName, address: applicationUser.Email);
        string token = await PrepVerificationAsync(applicationUser.Id, TimeSpan.FromMinutes(confirmationEmailConfig.Value.ConfirmationEmailExpiration));

        VerificationEmailModel verificationEmailModel =
            verificationEmailFactory.CreateModel(applicationUser.Username, language, token);

        Email email = await emailFactory.CreateAsync<VerificationEmailModel>(
            templateName: Templates.EmailTemplates.VerificationEmail,
            model: verificationEmailModel
        );

        _ = await emailClient.TrySendEmailAsync(recipients: [to], email);
    }

    public async Task<string> ConfirmEmailAsync(VerificationToken token, string language)
    {
        Log.Debug("Confirming email with token: {Token}", token);
        RedisValue userId = await redisConn.GetDatabase().StringGetAsync(token);

        string templateName = Templates.HtmlTemplates.EmailConfirmationFailed;
        EmailConfirmationStatus emailConfirmationStatus = emailConfirmationStatusModelFactory.CreateModel(
            userId: token.UserId,
            language: language
        );
        
        if (!userId.HasValue)
        {
            Log.Warning("Failed to confirm email with token: {Token}. Error: {Error}", token, "Invalid token");
            return await confirmationEmailTemplateFactory.CreateAsync<EmailConfirmationStatus>(templateName, emailConfirmationStatus);
        }

        Log.Debug("Token valid for user ID: {UserId}", userId);

        bool emailConfirmed = await authRepository.ConfirmEmailAsync((long)userId);
        if (emailConfirmed)
        {
            _ = await redisConn.GetDatabase().KeyDeleteAsync(token);
            templateName = Templates.HtmlTemplates.EmailConfirmed;
        }

        return await confirmationEmailTemplateFactory.CreateAsync<EmailConfirmationStatus>(templateName, emailConfirmationStatus);
    }

    /// <summary>
    /// Creates a token that is used to verify the users email. 
    /// The token contains the userID which can be extracted at any point
    /// </summary>
    /// <param name="userID"></param>
    /// <param name="expiry">The ttl that is set in redis</param>
    /// <returns></returns>
    private async Task<VerificationToken> PrepVerificationAsync(long userID, TimeSpan expiry)
    {
        VerificationToken token = VerificationToken.New(userID);

        _ = await redisConn.GetDatabase().StringSetAsync(key: token, value: userID, expiry);

        return token; 
    }
}