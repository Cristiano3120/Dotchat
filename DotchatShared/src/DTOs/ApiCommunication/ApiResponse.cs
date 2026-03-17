using System.Diagnostics.CodeAnalysis;

namespace DotchatShared.src.DTOs.ApiCommunication;

public sealed record ApiResponse<T>
{
    /// <summary>
    /// Gets a value indicating whether the operation completed successfully. 
    /// This value only states whether the request was processed successfully or not.
    /// </summary>
    /// <remarks> 
    /// This means that <see cref="IsSuccess"/> could be <see langword="true"/> 
    /// even tho <see cref="Error"/> is not <see langword="null"/> 
    /// </remarks>

    [MemberNotNullWhen(true, nameof(IsSuccess))]
    public bool IsSuccess { get; init; }

    /// <summary>
    /// Gets the returned data
    /// </summary>
    public T? Data { get; init; }

    /// <summary>
    /// Gets the error details associated with the API response, if an error occurred.
    /// </summary>
    /// <remarks>Use this property to inspect error information when the API operation does not succeed. If
    /// the operation is successful, this property is <see langword="null"/>.</remarks>
    public ApiError? Error { get; init; }
}