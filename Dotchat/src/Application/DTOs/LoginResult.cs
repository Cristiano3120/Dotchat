using DotchatServer.src.Application.DTOs.JwtModels;
using DotchatServer.src.Application.Enums;
using OneOf;

namespace DotchatServer.src.Application.DTOs;

public partial class LoginResult : OneOfBase<LoginResponse, LoginError>
{
    public LoginResult(OneOf<LoginResponse, LoginError> _) : base(_) { }

    public static implicit operator LoginResult(LoginResponse _) => new(_);
    public static implicit operator LoginResult(LoginError _) => new(_);
}

public sealed record LoginResponse(JwtClientData JwtClientData);
public sealed record LoginError(LoginErrorType LoginErrorType);