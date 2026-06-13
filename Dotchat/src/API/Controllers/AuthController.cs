using System.Net;
using DotchatServer.src.Application.DTOs;
using DotchatServer.src.Application.Interfaces;
using DotchatServer.src.Constants;
using DotchatShared.src.Constants;
using DotchatShared.src.DTOs.AuthRequests;
using DotchatShared.src.Enums;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace DotchatServer.src.API.Controllers;

[ApiController]
[Route($"{Endpoints.Base}/{Endpoints.AuthEndpoints.BaseAuth}")]
[EnableRateLimiting(policyName: RateLimitPolicies.Auth)]
public sealed class AuthController(IAuthService authService) : ControllerBase
{
    [HttpPost(Endpoints.AuthEndpoints.Login)]
    public async Task<IActionResult> LoginAsync([FromBody] LoginRequest loginRequest)
    {
        return Ok();
    }

    /// <summary>
    /// Registers a new user and returns an HTTP response indicating success or failure.
    /// </summary>
    /// <remarks>Determines client language and delegates to the authentication service; maps
    /// RegisterErrorType values to appropriate HTTP status codes.</remarks>
    /// <param name="registerRequest">Registration details, such as username, email, and password.</param>
    /// <returns>A task that produces an IActionResult: 200 OK with the registration result on success; 409 Conflict if the email
    /// or username is already taken; 503 Service Unavailable if the database is unavailable; 500 Internal Server Error
    /// for other failures.</returns>
    [HttpPost(Endpoints.AuthEndpoints.Register)]
    public async Task<IActionResult> RegisterAsync([FromBody] RegisterRequest registerRequest)
    {
        string lang = GetClientLanguage();
        RegisterResult registerResult = await authService.RegisterAsync(registerRequest, lang);
        return registerResult.Match(
            Ok, // Success case: return 200 OK with the result
            error => error.Type switch
            {
                RegisterErrorType.EmailTaken or RegisterErrorType.UsernameTaken => Conflict(error),
                RegisterErrorType.DbUnavailable => StatusCode((int)HttpStatusCode.ServiceUnavailable, error),
                _ => StatusCode((int)HttpStatusCode.InternalServerError, error)
            }
        );
    }

    /// <summary>
    /// Resends the account verification email and returns the rendered HTML template.
    /// </summary>
    /// <remarks>Renders the verification email using the client's preferred language and returns it as HTML
    /// content.</remarks>
    /// <param name="userID">The user identifier whose verification email will be resent.</param>
    /// <returns>An Ok containing the rendered HTML confirmation template.</returns>
    [HttpGet(Endpoints.AuthEndpoints.ResendConfirmation + "/{userId}", Name = Endpoints.AuthEndpoints.ResendConfirmation)]
    public async Task<IActionResult> ResendConfirmationAsync([FromRoute] long userId)
    {
        IHtmlRenderable template = await authService.ResendVerificationEmailAsync(userId, lang: GetClientLanguage());
        return Content(template.HtmlBody, ContentType.Html);
    }

    /// <summary>
    /// Tries to confirm the verification email associated with the provided verification token.
    /// </summary>
    /// <param name="token">The verification token to confirm the email.</param>
    /// <returns>Either a HTML view representing the result of the confirmation or a BadRequest if the token is invalid.</returns>
    [HttpGet(Endpoints.AuthEndpoints.ConfirmEmail + "/{token}", Name = Endpoints.AuthEndpoints.ConfirmEmail)]
    public async Task<IActionResult> ConfirmEmailAsync([FromRoute] string token)
    {
        string lang = GetClientLanguage();
        if (VerificationToken.TryParse(token, out VerificationToken verificationToken))
        {
            IHtmlRenderable template = await authService.ConfirmEmailAsync(verificationToken, lang);
            return Content(template.HtmlBody, ContentType.Html);
        }

        return BadRequest("Invalid token");
    }

    private string GetClientLanguage() => HttpContext.Features.Get<IRequestCultureFeature>()?.RequestCulture.Culture.TwoLetterISOLanguageName ?? "en";
}