namespace DotchatServer.src.Constants;

/// <summary>
/// Defines the rate limit policies used in the application.
/// Each property represents a specific rate limit policy that can be applied to API endpoints to control the number of requests a client can make within a certain time frame.
/// </summary>
internal static class RateLimitPolicies
{
    public const string Auth = "auth";
}