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
using MimeKit;
using Serilog;
using StackExchange.Redis;

namespace DotchatServer.src.Application.Services;

public sealed class AuthService(
    EmailConfirmationStatusModelFactory emailConfirmationStatusModelFactory,
    VerificationEmailFactory verificationEmailFactory,
    SnowflakeGenerator snowflakeGenerator,
    [FromKeyedServices(HashingAlgorithm.Argon2)] IHashingService hashingService,
    ITemplateFactory<string> htmlTemplateFactory,
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
        RefreshTokenInfo refreshTokenInfo = new(applicationUser.Id, hashingService.Hash(jwtData.RefreshToken), DateTimeOffset.UtcNow.Add(jwtData.Expiery));

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
        {
            return "User not found";//TODO: Return Template 
                                    //Maybe combine alle factories mit nem Inteface 
                                    //Mach Email expiery time in die appsettings.json
                                    //Mach das die time also sowas wie DateTime in den Templates localized ist
                                    //Guck Claude wegen Template und überarbeite das
        }

        await SendVerificationEmailAsync(applicationUser, language); //Maybe mach das das nen bool returned
        //TODO: Return Template mach ne ResendConfirmationEmailModelFactory 
        return htmlTemplateFactory.CreateAsync<ResendConfirmationEmailSuccessfulModel>(Templates.HtmlTemplates.ResendConfirmation, );
    }

    private async Task SendVerificationEmailAsync(ApplicationUser applicationUser, string language)
    {
        MailboxAddress to = new(name: applicationUser.DisplayName, address: applicationUser.Email);
        TimeSpan expiery = TimeSpan.FromMinutes(15);
        string token = await PrepVerificationAsync(applicationUser.Id, expiery);

        VerificationEmailModel verificationEmailModel =
            verificationEmailFactory.CreateModel(applicationUser.Username, language, token, expiery);

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
            userId: userId.IsNull ? TryExtractUserIdFromToken(token) : (long)userId,
            language: language
        );
        
        if (!userId.HasValue)
        {
            Log.Warning("Failed to confirm email with token: {Token}. Error: {Error}", token, "Invalid token");
            return await htmlTemplateFactory.CreateAsync<EmailConfirmationStatus>(templateName, emailConfirmationStatus);
        }

        Log.Debug("Token valid for user ID: {UserId}", userId);

        bool emailConfirmed = await authRepository.ConfirmEmailAsync((long)userId);
        if (emailConfirmed)
        {
            _ = await redisConn.GetDatabase().KeyDeleteAsync(token);
            templateName = Templates.HtmlTemplates.EmailConfirmed;
        }

        return await htmlTemplateFactory.CreateAsync<EmailConfirmationStatus>(templateName, emailConfirmationStatus);
    }

    private async Task<string> PrepVerificationAsync(long userID, TimeSpan expiery)
    {
        byte[] data = Encoding.UTF8.GetBytes(Guid.NewGuid().ToString() + userID.ToString());
        string token = Convert.ToBase64String(data);

        _ = await redisConn.GetDatabase().StringSetAsync(key: token, value: userID, expiery);

        return token;
    }

    private static long TryExtractUserIdFromToken(string token)
    {
        try
        {
            byte[] data = Convert.FromBase64String(token);
            string result = Encoding.UTF8.GetString(data);

            return long.Parse(result[Guid.NewGuid().ToString().Length..]);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to extract user ID from token: {Token}", token);
            return -1; // Return an invalid user ID to indicate failure
        }
    }
}