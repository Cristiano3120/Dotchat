using DotchatServer.src.Application.Enums;
using DotchatShared.src.DTOs;
using OneOf;

namespace DotchatServer.src.Application.DTOs;

public partial class LoginResult : OneOfBase<LoginSuccess, LoginError>
{
    public LoginResult(OneOf<LoginSuccess, LoginError> _) : base(_) { }

    public static implicit operator LoginResult(LoginSuccess _) => new(_);
    public static implicit operator LoginResult(LoginError _) => new(_);
}

public sealed record LoginSuccess(JwtClientData JwtClientData);
public sealed record LoginError(LoginErrorType LoginErrorType);