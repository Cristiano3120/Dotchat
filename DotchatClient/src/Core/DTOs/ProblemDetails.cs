using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DotchatClient.src.Core.DTOs;

internal sealed record ProblemDetails
{
    public string? Type { get; set; }
    public string? Title { get; set; }
    public HttpStatusCode Status { get; set; }
    public string? Detail { get; set; }
    public string? Instance { get; set; }

    [JsonExtensionData]
    public Dictionary<string, JsonElement>? Extensions { get; set; }
}