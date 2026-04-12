using DotchatServer.src.Application.DTOs;
using DotchatServer.src.Application.Services;
using DotchatServer.src.Constants;
using DotchatShared.src.Constants;
using DotchatShared.src.DTOs.AuthRequests;
using DotchatShared.src.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System.Net;

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
        RegisterResult registerResult = await authService.RegisterAsync(registerRequest);
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

    [Authorize]
    [HttpGet(Endpoints.AuthEndpoints.RequestVerification)]
    public async Task<IActionResult> RequestVerificationAsync([FromBody] int userID)
    {
        return Ok();
    }

    [HttpGet(Endpoints.AuthEndpoints.Verify)]
    public async Task<IActionResult> VerifyAsync([FromQuery] int userID, [FromQuery] string token)
    {
        return Ok(); //look at linear issue
    }
}