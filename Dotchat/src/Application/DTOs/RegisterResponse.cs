using DotchatServer.src.Application.DTOs.JwtModels;
using DotchatShared.src.Enums;

using OneOf;

namespace DotchatServer.src.Application.DTOs;

/// <summary>
/// <c>UNION:</c>
/// Represents the result of a registration operation, which can be either a successful response or an error.
/// </summary>
/// <remarks>This type encapsulates both successful and error outcomes for registration, allowing methods to
/// return a single result type. Use pattern matching or type checking to determine whether the result is a success or
/// an error.</remarks>
public partial class RegisterResult : OneOfBase<RegisterResponse, RegisterError>
{
    private RegisterResult(OneOf<RegisterResponse, RegisterError> _) : base(_) { }

    public static implicit operator RegisterResult(RegisterResponse _) => new(_);
    public static implicit operator RegisterResult(RegisterError _) => new(_);
}

/// <summary>
/// Represents a successful registration response containing JWT client data.
/// </summary>
/// <param name="JwtClientData">The JWT client data associated with the successful registration.</param>
public sealed record RegisterResponse(JwtClientData JwtClientData);

/// <summary>
/// Represents an error that occurred during the registration process, containing the type of error that occurred.
/// </summary>
/// <param name="Type">The type of error that occurred during registration.</param>
public sealed record RegisterError(RegisterErrorType Type);