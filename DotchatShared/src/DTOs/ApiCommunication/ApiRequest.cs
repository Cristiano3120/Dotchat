namespace DotchatShared.src.DTOs.ApiCommunication;

/// <summary>
/// Represents a request to an API, encapsulating the payload to be sent.
/// </summary>
/// <param name="Payload">The data to include in the API request. This object represents the request body and may be of any type required by
/// the API endpoint</param>
internal sealed record ApiRequest(object? Payload);