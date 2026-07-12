using System.Text.Json.Serialization;

namespace DotchatClient.src.Core.DTOs;

[JsonSerializable(typeof(ApiError))]
[JsonSerializable(typeof(ProblemDetails))]
internal partial class AppJsonContext : JsonSerializerContext { }