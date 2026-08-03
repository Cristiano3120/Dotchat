using DotchatClient.src.Application.Interfaces;
using DotchatClient.src.Core;
using DotchatClient.src.Core.DTOs;
using DotchatShared.src.Constants;
using DotchatShared.src.DTOs;
using DotchatShared.src.DTOs.AuthRequests;
using DotchatShared.src.Interfaces;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace DotchatClient.Components.Pages;

public partial class Register(IHttpApiClient httpApiClient, IDeviceInfoService deviceInfoService, IUrlBuilder urlBuilder)
{
    private const string GitHubSvg = ImagePaths.GitHubSvg;
    private const string EyeSvg = ImagePaths.PasswordEyeSvg;
    private const string ClosedEyeSvg = ImagePaths.ClosedPasswordEyeSvg;
    private const string GoogleSvg = ImagePaths.GoogleSvg;
    private const string DotchatSvg = ImagePaths.DotchatSvg;
    private const int MaxDisplayNameLength = 24;
    private const int MaxUsernameLength = 24;
    private const int MaxBioLength = 250;

    private string Email = "Cristianocx7@gmail.com"; //Fields leer? Acc existiert bereits? 
    private string Username = "Cristiano";
    private string DisplayName = "Cris";
    private string Password = "Cristiano2007!";
    private string Bio = string.Empty;
    private DateOnly? Birthday = DateOnly.FromDateTime(DateTime.UtcNow);

    private string? ErrorMessage;
    private bool ShowPassword;
    private bool IsLoading;

    private readonly string _today = DateTime.UtcNow.Date.ToString("yyyy-MM-dd");
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
        
        RegisterRequest registerRequest = new
        (
            Email: Email,
            Password: Password,
            Username: Username,
            Platform: deviceInfoService.GetPlatform(),
            Birthday: Birthday,
            DeviceId: deviceInfoService.GetDeviceId(),
            DisplayName: DisplayName,
            Bio: Bio,
            DeviceName: deviceInfoService.GetDeviceName()
        ); 

        string registerUrl = urlBuilder.AddUrl(Endpoints.AuthEndpoints.RegisterEndpoint).Build();
        Result<JwtClientData, ApiError> result = await httpApiClient.PostAsync<RegisterRequest, JwtClientData>(registerUrl, registerRequest);
        if (result.IsOperationSuccess)
        {
            ErrorMessage = "True"; //TODO: Mach local field test also check ob leer/valid via regex bei email und implement 
                                   // vernünftige error messages musst validation errors die vom server kopmmen mappen
            //Save Jwt
            //Navigate to /home
        }
        else
        {
            ErrorMessage = $"Fehler beim Anmelden: {result.Error.Title}"; //TODO: Localize
        }

        IsLoading = false;
        StateHasChanged();
    }
}