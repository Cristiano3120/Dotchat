using DotchatShared.src.Enums;
using System.Net;

namespace DotchatClient.src.Core.DTOs;

/// <summary>
/// 
/// </summary>
/// <param name="HttpStatusCode">The HTTP status code. 0 if there is no HTTP response for example in case of network errors</param>
/// <param name="ErrorCode"></param>
/// <param name="Title"></param>
internal readonly record struct ApiError(HttpStatusCode HttpStatusCode, ApiErrorCode ErrorCode, string Title);