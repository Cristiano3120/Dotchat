using System.Text.Json.Serialization;

namespace DotchatClient.src.Core.DTOs;

internal sealed record GoogleClientSecret
{
    [JsonPropertyName("client_id")]
    public string ClientId { get; init; } = string.Empty;
    [JsonPropertyName("project_id")]
    public string ProjectId { get; init; } = string.Empty;
    [JsonPropertyName("auth_uri")]
    public string AuthUri { get; init; } = string.Empty;
    [JsonPropertyName("token_uri")]
    public string TokenUri { get; init; } = string.Empty;
    [JsonPropertyName("auth_provider_x509_cert_url")]
    public string AuthProviderX509CertUrl { get; init; } = string.Empty;
    [JsonPropertyName("client_secret")]
    public string ClientSecret { get; init; } = string.Empty;
    [JsonPropertyName("redirect_uris")]
    public IEnumerable<string> RedirectUris { get; init; } = [];
}