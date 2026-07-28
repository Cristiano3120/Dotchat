using DotchatClient.src.Core;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace DotchatClient.Components.Pages;

public partial class Register
{
    private const string GitHubSvg = ImagePaths.GitHubSvg;
    private const string EyeSvg = ImagePaths.PasswordEyeSvg;
    private const string ClosedEyeSvg = ImagePaths.ClosedPasswordEyeSvg;
    private const string GoogleSvg = ImagePaths.GoogleSvg;
    private const string DotchatSvg = ImagePaths.DotchatSvg;
    private const int MaxDisplayNameLength = 24;
    private const int MaxUsernameLength = 24;
    private const int MaxBioLength = 250;

    private string Email = string.Empty;
    private string Username = string.Empty;
    private string DisplayName = string.Empty;
    private string Password = string.Empty;
    private string Bio = string.Empty;
    private DateTime? Birthday;

    private string? ErrorMessage;
    private bool ShowPassword;
    private bool IsLoading;

    private readonly string _today = DateTime.Today.ToString("yyyy-MM-dd");
    private ElementReference _birthdayInput;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            try
            {
                await JS.InvokeVoidAsync("limitDateYear", _birthdayInput);
            }
            catch (InvalidOperationException) { } //WebView not ready :(
        }
    }

    /// <summary>
    /// Calculates the state of the input count. For example if a textbox allows 24 chars the color of the count
    /// will change at the nearLimitThreshold(80%) and at the max ammount of chars(100%)
    /// </summary>
    private static string GetCountClass(int current, int max)
    {
        float nearLimitThreshold = 0.8f;
        if (current >= max)
        {
            return "at-limit";
        }

        return current >= max * nearLimitThreshold 
            ? "near-limit" 
            : "";
    }

    private async Task HandleRegisterAsync()
    {
        IsLoading = true;
        StateHasChanged();

        // TODO: Register-Request an Backend

        IsLoading = false;
        StateHasChanged();
    }
}