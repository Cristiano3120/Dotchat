using DotchatShared.src.DTOs;
using System.Text.Json.Serialization;

namespace DotchatClient.src.Core.DTOs;

[JsonSerializable(typeof(ApiError))]

[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase, PropertyNameCaseInsensitive = true)]
[JsonSerializable(typeof(ProblemDetails))]

[JsonSerializable(typeof(JwtClientData))]
internal partial class AppJsonContext : JsonSerializerContext { }