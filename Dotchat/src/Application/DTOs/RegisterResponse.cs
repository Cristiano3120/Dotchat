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

public sealed record RegisterResponse(JwtClientData JwtClientData);

public sealed record RegisterError(RegisterErrorType Type);