using DotchatServer.src.Constants;
using DotchatShared.src.Constants;
using DotchatShared.src.DTOs.AuthRequests;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace DotchatServer.src.API.Controllers;

[ApiController]
[Route($"{Endpoints.Base}/{Endpoints.AuthEndpoints.BaseAuth}")]
public sealed class AuthController() : ControllerBase
{
    [HttpPost(Endpoints.AuthEndpoints.Login)]
    [EnableRateLimiting(policyName: RateLimitPolicies.Auth)]
    public async Task<IActionResult> LoginAsync([FromBody] LoginRequest loginRequest)
    {
        return Ok();
    }

    [HttpPost(Endpoints.AuthEndpoints.Register)]
    [EnableRateLimiting(policyName: RateLimitPolicies.Auth)]
    public async Task<IActionResult> RegisterAsync([FromBody] RegisterRequest registerRequest)
    {
        return Ok();
    }
}