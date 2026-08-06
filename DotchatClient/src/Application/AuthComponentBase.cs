using DotchatClient.src.Core;
using DotchatShared.src.Constants;
using Microsoft.AspNetCore.Components;

namespace DotchatClient.src.Application;

public abstract class AuthComponentBase : ComponentBase
{
    protected const string GitHubSvg = ImagePaths.GitHubSvg;
    protected const string EyeSvg = ImagePaths.PasswordEyeSvg;
    protected const string ClosedEyeSvg = ImagePaths.ClosedPasswordEyeSvg;
    protected const string GoogleSvg = ImagePaths.GoogleSvg;
    protected const string DotchatSvg = ImagePaths.DotchatSvg;

    protected const int MinPasswordLength = AuthRequestRules.MinPasswordLength;
    protected const int MaxPasswordLength = AuthRequestRules.MaxPasswordLength;

    protected string Email { get; set; } = string.Empty;
    protected string Password { get; set; } = string.Empty;
    protected string? ErrorMessage { get; set; }
    protected bool ShowPassword { get; set; }
    protected bool IsLoading { get; set; }

    protected void TogglePasswordVisibility() => ShowPassword = !ShowPassword;
}