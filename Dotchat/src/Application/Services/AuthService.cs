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
using DotchatShared.src.Constants;
using DotchatShared.src.DTOs.AuthRequests;
using DotchatShared.src.Enums;
using Microsoft.Extensions.Options;
using MimeKit;
using Serilog;

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
        RefreshTokenInfo refreshTokenInfo = new(applicationUser.Id, hashingService.Hash(jwtData.RefreshToken), DateTimeOffset.UtcNow.Add(jwtData.Expiry));

        RegisterErrorType registrationResult = await authRepository.CompleteRegistrationAsync(applicationUser, refreshTokenInfo, registerRequest.Password);
        if (registrationResult != RegisterErrorType.None)
        {
            return new RegisterError(registrationResult);
        }

        //We dont care if the email was sent successfully or not at this point, the user is registered either way and can request a new verification email if needed
        TrySendVerificationEmailAsync(applicationUser, language).FireAndForget();
        return new RegisterResponse(jwtData);
    }

    /// <summary>
    /// Resends a verification email to the user with the given userID. 
    /// If the user's email is already confirmed, a template indicating that is returned instead.
    /// </summary>
    /// <param name="userID">The ID of the user to resend the verification email to.</param>
    /// <param name="resendUrl">The URL to include in the verification email for resending confirmation.</param>
    /// <param name="lang">The language or culture code used to localize the returned status template.</param>
    /// <returns>An IHtmlRenderable containing the localized email confirmation status view.</returns>
    public async Task<IHtmlRenderable> ResendVerificationEmailAsync(long userID, string lang)
    {
        ApplicationUser? applicationUser = await authRepository.GetUserByIdAsync(userID);

        if (applicationUser is null)
        {   //No need to return a template since a normal user can´t hit this path anyway
            return new RawHtmlTemplate("User not found. The link you clicked or the request you made contained a faulty userID");
        }

        if (applicationUser.EmailConfirmed)
        {
            return await CreateEmailConfirmedTemplateAsync(lang);
        }

        //We dont care if the email was sent successfully or not at this point, the user is registered either way and can request a new verification email if needed
        TrySendVerificationEmailAsync(applicationUser, lang).FireAndForget();

        return await CreateResendConfirmationTemplateAsync(applicationUser, BuildResendUrl(userID), lang);
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
        Log.Debug("Confirming email with token: {Token}", token);
        string resendUrl = BuildResendUrl(token.UserId);

        Result<bool>? emailConfirmed = null;
        if (await redisCache.ExistsAsync(token))
        {
            //If deleting the token fails, it will just expire after a certain amount of time, so we can ignore failures here
            redisCache.DeleteAsync(token).FireAndForget();
            emailConfirmed = await authRepository.ConfirmEmailAsync(token.UserId);

            if (!emailConfirmed.IsOperationSuccess)
            {
                return await CreateEmailConfirmationFailedServerFaultTemplateAsync(resendUrl, language);
            }

            if (emailConfirmed.Value)
            {
                return await CreateEmailConfirmedTemplateAsync(language);
            }
        }

        emailConfirmed ??= await authRepository.ConfirmEmailAsync(token.UserId);
        if (!emailConfirmed.IsOperationSuccess)
        {
            return await CreateEmailConfirmationFailedServerFaultTemplateAsync(resendUrl, language);
        }

        return emailConfirmed.Value
            ? await CreateEmailConfirmedTemplateAsync(language)
            : await CreateEmailConfirmationFailedTemplateAsync(resendUrl, language);
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
    private async Task<(bool success, VerificationToken token)> PrepVerificationAsync(long userID)
    {
        VerificationToken token = VerificationToken.New(userID);
        TimeSpan expiry = TimeSpan.FromMinutes(emailConfig.Value.ConfirmationEmailExpiration);

        if (!await redisCache.SetAsync(key: token, value: userID, expiry))
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

    private string BuildResendUrl(long userId) => urlBuilder.AddUrl(Endpoints.AuthEndpoints.ResendConfirmationEndpoint)
            .AddRouteParam(userId.ToString()).Build();

    private string BuildConfirmationUrl (VerificationToken token) => urlBuilder.AddUrl(Endpoints.AuthEndpoints.ConfirmEmailEndpoint)
            .AddRouteParam(token).Build();
}