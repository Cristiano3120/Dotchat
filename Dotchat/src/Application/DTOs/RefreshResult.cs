using DotchatShared.src.DTOs;
using DotchatShared.src.Enums;
using OneOf;

namespace DotchatServer.src.Application.DTOs;

/// <summary>
/// <c>UNION:</c>
/// </summary>
public partial class RefreshResult : OneOfBase<RefreshSuccess, RefreshError>
{
    private RefreshResult(OneOf<RefreshSuccess, RefreshError> _) : base(_) { }

    public static implicit operator RefreshResult(RefreshSuccess _) => new(_);
    public static implicit operator RefreshResult(RefreshError _) => new(_);
}

public sealed record RefreshSuccess(AccessTokenInfo AccessTokenInfo);
public sealed record RefreshError(RefreshErrorType RefreshErrorType);
