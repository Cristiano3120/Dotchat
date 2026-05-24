using System.Net;

using DotchatServer.src.Application.DTOs;
using DotchatServer.src.Application.Services;
using DotchatServer.src.Constants;

using DotchatShared.src.Constants;
using DotchatShared.src.DTOs.AuthRequests;
using DotchatShared.src.Enums;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace DotchatServer.src.API.Controllers;

[ApiController]
[Route($"{Endpoints.Base}/{Endpoints.AuthEndpoints.BaseAuth}")]
[EnableRateLimiting(policyName: RateLimitPolicies.Auth)]
public sealed class AuthController(AuthService authService) : ControllerBase
{
    [HttpPost(Endpoints.AuthEndpoints.Login)]
    public async Task<IActionResult> LoginAsync([FromBody] LoginRequest loginRequest)
    {
        return Ok();
    }

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

    [HttpGet(Endpoints.AuthEndpoints.ResendConfirmation)]
    public async Task<IActionResult> ResendConfirmationAsync([FromQuery] long userID)
        => Content(await authService.ResendVerificationEmailAsync(userID, language: GetClientLanguage()), contentType: "text/html"); 

    [HttpGet(Endpoints.AuthEndpoints.ConfirmEmail)]
    public async Task<IActionResult> ConfirmEmailAsync([FromQuery] string token)
    {
        string lang = GetClientLanguage();
        if (VerificationToken.TryParse(token, out VerificationToken verificationToken))
        {
            return Content(await authService.ConfirmEmailAsync(verificationToken, lang), contentType: "text/html"); 
        }

        return BadRequest("Invalid token");
    }

    private string GetClientLanguage()
        => HttpContext.Features.Get<IRequestCultureFeature>()?.RequestCulture.Culture.TwoLetterISOLanguageName ?? "en";
    
}