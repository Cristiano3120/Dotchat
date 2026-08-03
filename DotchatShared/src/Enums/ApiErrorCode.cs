namespace DotchatShared.src.Enums;

public enum ApiErrorCode : byte
{
    /// <summary>
    /// Implies that the error code is the same as the HTTP status code. 
    /// This is used when the API does not provide a specific error code, and the HTTP status code itself is sufficient to describe the error.
    /// </summary>
    SameAsStatusCode,

    //Client-side errors
    NetworkUnavailable,
    Timeout,
    DeserializationError,

    //Server-side errors
    WrongCredentials,
    DbUnavailable,

    /// <summary>
    /// Accours when a request fails validation, such as when required fields are missing. 
    /// This is automatically handled by the framework when using Data Annotations in DTOs, resulting in a 400 Bad Request response.
    /// </summary>
    ValidationFailed,
}