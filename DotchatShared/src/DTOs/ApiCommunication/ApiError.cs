using System.Net;

namespace DotchatShared.src.DTOs.ApiCommunication;

/// <summary>
/// Represents an error response from an API, including the HTTP status code and a descriptive error message.
/// </summary>
/// <param name="HttpStatusCode">The HTTP status code associated with the error response.</param>
/// <param name="ErrorMsg">A message that describes the error returned by the API.</param>
public sealed record ApiError(HttpStatusCode HttpStatusCode, string ErrorMsg);