using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using DotchatServer.src.Application.Commands;
using DotchatServer.src.Application.DTOs;
using DotchatServer.src.Application.DTOs.EmailModels;
using DotchatServer.src.Application.DTOs.Emails;
using DotchatServer.src.Application.DTOs.JwtModels;
using DotchatServer.src.Application.DTOs.TemplateModels;
using DotchatServer.src.Application.Enums;
using DotchatServer.src.Application.Factories;
using DotchatServer.src.Application.Interfaces;
using DotchatServer.src.Application.Interfaces.Security;
using DotchatServer.src.Core.Config;
using DotchatServer.src.Core.Entities;
using DotchatServer.src.Core.Extensions;
using DotchatServer.src.Core.Interfaces;
using DotchatServer.src.Core.Templates;
using DotchatServer.src.Infrastructure;
using DotchatServer.src.Infrastructure.Persistence.Repos;
using DotchatShared.src.Constants;
using DotchatShared.src.DTOs;
using DotchatShared.src.DTOs.AuthRequests;
using DotchatShared.src.Enums;
using Microsoft.Extensions.Options;
using MimeKit;
using Serilog;
using StackExchange.Redis;

namespace DotchatServer.src.Application.Services;

/// <summary>
/// Service responsible for handling user registration, email confirmation, and resending verification emails.
/// </summary>
internal sealed class AuthService(
    ResendConfirmationEmailModelFactory resendConfirmationEmailModelFactory,
    EmailConfirmationStatusModelFactory emailConfirmationStatusModelFactory,
    EmailConfirmationFailedModelFactory emailConfirmationFailedModelFactory,
    VerificationEmailFactory verificationEmailFactory,
    SnowflakeGenerator snowflakeGenerator,
    [FromKeyedServices(TemplateFactoryKey.ResendConfirmation)] ITemplateFactory<HtmlTemplate> resendConfirmationEmailTemplateFactory,
    [FromKeyedServices(TemplateFactoryKey.Confirmation)] ITemplateFactory<HtmlTemplate> confirmationEmailTemplateFactory,
    [FromKeyedServices(HashingAlgorithm.Argon2)] IHashingService hashingService,
    ITemplateFactory<Email> emailFactory,
    IAuthRepository authRepository,
    IEmailClient emailClient,
    IRedisCache redisCache,
    IJwtService jwtService,
    IUrlBuilder urlBuilder,
    IOptions<ConfirmationEmailConfig> emailConfig) : IAuthService
{
    public async Task<LoginResult> LoginAsync(LoginCommand loginRequest)
    {
        try
        {
            ApplicationUser? user = await authRepository.FindUserByEmailAsync(loginRequest.Email);
            byte[] passwordHash = hashingService.Hash(loginRequest.Password);

            if (user is null || !CryptographicOperations.FixedTimeEquals(user.PasswordHash, passwordHash))
            {
                return new LoginError(LoginErrorType.WrongCredentials);
            }

            JwtClientData jwtClientData = jwtService.GenerateToken(user.Id, user.Email);
            byte[] tokenHash = hashingService.Hash(jwtClientData.AccessToken);
            RefreshTokenInfo refreshTokenInfo = new(expiry: jwtClientData.Expiry)
            {
                UserId = user.Id,
                DeviceId = loginRequest.DeviceId,
                Platform = loginRequest.Platform,
                TokenHash = tokenHash,
            };

            await authRepository.UpsertRefreshTokenAsync(refreshTokenInfo);
            return new LoginResponse(jwtClientData);
        }
        catch (Exception ex)
        {
            Log.Error("Error trying to login: {ex}", ex);
            return new LoginError(LoginErrorType.DbException);
        }
    }

    public async Task<RegisterResult> RegisterAsync(RegisterCommand registerRequest, string language)
    {
        try
        {
            if (await authRepository.CheckIfEmailExistsAsync(registerRequest.Email))
            {
                return new RegisterError(RegisterErrorType.EmailTaken);
            }

            if (await authRepository.CheckIfUsernameExistsAsync(registerRequest.Username))
            {
                return new RegisterError(RegisterErrorType.UsernameTaken);
            }

            ApplicationUser user = new()
            {
                Id = snowflakeGenerator.NextId(),
                Email = registerRequest.Email,
                Username = registerRequest.Username,
                PasswordHash = hashingService.Hash(registerRequest.Password),
                DisplayName = registerRequest.DisplayName,
                Birthday = registerRequest.Birthday,
            };
            JwtClientData jwtClientData = jwtService.GenerateToken(user.Id, registerRequest.Email);
            RefreshTokenInfo tokenInfo = new(jwtClientData.Expiry, registerRequest.DeviceName)
            {
                UserId = user.Id,
                DeviceId = registerRequest.DeviceId,
                Platform = registerRequest.Platform,
                TokenHash = hashingService.Hash(jwtClientData.RefreshToken),
            };

            Log.Information("Attempting to register user: {applicationUser}", user);
            await authRepository.RegisterUserAsync(user, tokenInfo);
            TrySendVerificationEmailAsync(user, language).FireAndForget();

            return new RegisterResponse(jwtClientData);
        }
        catch (Exception ex)
        {
            Log.Error("Error while trying to create a user: {ex}", ex);
            return new RegisterError(RegisterErrorType.DbUnavailable);
        }
    }

    /// <summary>
    /// Resends a verification email to the user with the given userID. 
    /// If the user's email is already confirmed, a template indicating that is returned instead.
    /// </summary>
    /// <param name="userID">The ID of the user to resend the verification email to.</param>
    /// <param name="resendUrl">The URL to include in the verification email for resending confirmation.</param>
    /// <param name="lang">The language or culture code used to localize the returned status template.</param>
    /// <returns>An IHtmlRenderable containing the localized email confirmation status view.</returns>
    public async Task<IHtmlRenderable> ResendVerificationEmailAsync(Snowflake userID, string lang)
    {
        Result<ApplicationUser?, Exception> result = await authRepository.GetUserByIdAsync(userID);

        if (!result.IsOperationSuccess)
        {
            return new RawHtmlTemplate("Internal Server Error :( Try again later");
        }

        ApplicationUser? user = result.Value;
        if (user is null)
        {   //No need to return a template since a normal user can´t hit this path anyway
            return new RawHtmlTemplate("User not found. The link you clicked or the request you made contained a faulty userID");
        }

        if (user.EmailConfirmed)
        {
            return await CreateEmailConfirmedTemplateAsync(lang);
        }

        //We dont care if the email was sent successfully or not at this point, the user is registered either way and can request a new verification email if needed
        TrySendVerificationEmailAsync(user, lang).FireAndForget();

        return await CreateResendConfirmationTemplateAsync(user, BuildResendUrl(userID), lang);
    }

    /// <summary>
    /// Confirms a user's email using the provided verification token and returns a localized HTML status view.
    /// </summary>
    /// <remarks>Validates the token in Redis, logs failures, invokes the authentication repository to mark
    /// the email as confirmed, deletes the token from cache on success, and selects the appropriate HTML template for
    /// the result.</remarks>
    /// <param name="token">Verification token containing the user identifier and token value used to confirm the email.</param>
    /// <param name="language">Language or culture code used to localize the returned status template.</param>
    /// <returns>An IHtmlRenderable containing the localized email confirmation status view.</returns>
    public async Task<IHtmlRenderable> ConfirmEmailAsync(VerificationToken token, string language)
    {
        string resendUrl = BuildResendUrl(token.UserId);
        try
        {
            Log.Debug("Confirming email with token: {Token}", token);

            if (await redisCache.ExistsAsync(token))
            {
                //If deleting the token fails, it will just expire after a certain amount of time, so we can ignore failures here
                redisCache.DeleteAsync(token).FireAndForget();
            }

            bool emailConfirmedResult = await authRepository.ConfirmEmailAsync(token.UserId);
            if (emailConfirmedResult)
            {
                return await CreateEmailConfirmedTemplateAsync(language);
            }

            return await CreateEmailConfirmationFailedTemplateAsync(resendUrl, language);
        }
        catch (Exception ex)
        {
            Log.Error("Error accoured while trying to confirm the email: {ex}", ex);
            return await CreateEmailConfirmationFailedServerFaultTemplateAsync(resendUrl, language);
        }
    }

    /// <summary>
    /// Sends a verification email to the user. 
    /// </summary>
    /// <returns>Returns whether the email was successfully sent.</returns>
    private async Task<bool> TrySendVerificationEmailAsync(ApplicationUser applicationUser, string language)
    {
        MailboxAddress to = new(name: applicationUser.DisplayName, address: applicationUser.Email);
        (bool success, VerificationToken token) = await PrepVerificationAsync(applicationUser.Id);

        if (!success)
        {
            Log.Warning("Failed to prepare verification for user ID: {UserID}. Email not sent.", applicationUser.Id);
            return false;
        }

        string confirmationUrl = BuildConfirmationUrl(token);
        Email email = await CreateVerificationEmailAsync(token, applicationUser.Username, confirmationUrl, language);
        success = await emailClient.TrySendEmailAsync(recipients: [to], email);
        if (!success)
        {
            Log.Warning("Failed to send verification email to user ID: {UserID} at email: {Email}", applicationUser.Id, applicationUser.Email);
            return false;
        }

        Log.Debug("Sent verification email to {Email} with token: {Token}", applicationUser.Email, token);
        return true;
    }

    /// <summary>
    /// Creates a token that is used to verify the users email. 
    /// The token contains the userID which can be extracted at any point
    /// </summary>
    /// <param name="userID"></param>
    /// <param name="expiry">The ttl that is set in redis</param>
    /// <returns></returns>
    private async Task<(bool success, VerificationToken token)> PrepVerificationAsync(Snowflake userID)
    {
        VerificationToken token = VerificationToken.New(userID);
        TimeSpan expiry = TimeSpan.FromMinutes(emailConfig.Value.ConfirmationEmailExpiration);

        if (!await redisCache.SetAsync(key: token, value: userID.Value, expiry))
        {
            Log.Warning("Failed to set verification token in Redis for user ID: {UserID}", userID);
            return (false, VerificationToken.Empty);
        }

        return (true, token);
    }

    private async Task<IHtmlRenderable> CreateEmailConfirmedTemplateAsync(string language)
    {
        EmailConfirmedModel model = emailConfirmationStatusModelFactory.CreateModel(language);
        return await confirmationEmailTemplateFactory.CreateAsync(Templates.HtmlTemplates.EmailConfirmed, model);
    }

    private async Task<IHtmlRenderable> CreateEmailConfirmationFailedTemplateAsync(string resendUrl, string language)
    {
        EmailConfirmationFailedModel model = emailConfirmationFailedModelFactory.CreateModel(resendUrl, language);
        return await confirmationEmailTemplateFactory.CreateAsync(Templates.HtmlTemplates.EmailConfirmationFailed, model);
    }

    private async Task<IHtmlRenderable> CreateEmailConfirmationFailedServerFaultTemplateAsync(string resendUrl, string language)
    {
        EmailConfirmationFailedModel model = emailConfirmationFailedModelFactory.CreateModel(resendUrl, language);
        return await confirmationEmailTemplateFactory.CreateAsync(Templates.HtmlTemplates.EmailConfirmationFailedServerError, model);
    }

    private async Task<IHtmlRenderable> CreateResendConfirmationTemplateAsync(ApplicationUser user, string resendUrl, string language)
    {
        ResendConfirmationEmailModel model = resendConfirmationEmailModelFactory.CreateModel(user.DisplayName, resendUrl, language);
        return await resendConfirmationEmailTemplateFactory.CreateAsync(Templates.HtmlTemplates.ResendConfirmation, model);
    }

    private async Task<Email> CreateVerificationEmailAsync(VerificationToken token, string displayName, string confirmationUrl, string language)
    {
        VerificationEmailModel verificationEmailModel = verificationEmailFactory.CreateModel(displayName, confirmationUrl, language, token);
        return await emailFactory.CreateAsync(Templates.EmailTemplates.VerificationEmail, verificationEmailModel);
    }

    private string BuildResendUrl(Snowflake userId) => urlBuilder.AddUrl(Endpoints.AuthEndpoints.ResendConfirmationEndpoint)
            .AddRouteParam(userId.ToString()).Build();

    private string BuildConfirmationUrl(VerificationToken token) => urlBuilder.AddUrl(Endpoints.AuthEndpoints.ConfirmEmailEndpoint)
            .AddRouteParam(token).Build();
}