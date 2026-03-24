using DotchatServer.src.Application.Services;
using DotchatServer.src.Constants;
using DotchatShared.src.Constants;
using DotchatShared.src.DTOs.AuthRequests;
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
        await authService.RegisterAsync(registerRequest);
        return Ok();
    }
}