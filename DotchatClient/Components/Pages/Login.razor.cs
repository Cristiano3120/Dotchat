using DotchatClient.src.Application.Interfaces;
using DotchatClient.src.Application.Services;
using DotchatClient.src.Core;
using DotchatClient.src.Core.DTOs;
using DotchatServer.src.Application.DTOs.JwtModels;
using DotchatShared.src.Constants;
using DotchatShared.src.DTOs;
using DotchatShared.src.DTOs.AuthRequests;
using DotchatShared.src.Interfaces;

namespace DotchatClient.Components.Pages;

public partial class Login(IHttpApiClient httpApiClient, IDeviceInfoService deviceInfoService, IUrlBuilder urlBuilder, Js js)
{
    private readonly string GitHubSvg = ImagePaths.GitHubSvg;
    private readonly string EyeSvg = ImagePaths.PasswordEyeSvg;
    private readonly string ClosedEyeSvg = ImagePaths.ClosedPasswordEyeSvg;
    private readonly string GoogleSvg = ImagePaths.GoogleSvg;
    private readonly string DotchatSvg = ImagePaths.DotchatSvg; //TODO:Make Register.razor partial (put code into seperate file)
    private string Email { get; set; } = string.Empty;          //TODO: Make Btn press anims
    private string Password { get; set; } = string.Empty;       //TODO: Bio box muss selber expanden nicht großziehbar sein
    private string ErrorMessage { get; set; } = string.Empty;
    private bool ShowPassword { get; set; }
    private bool IsLoading { get; set; }

    private async Task HandleLoginAsync()
    {
        if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password))
        {
            ErrorMessage = "Bitte E-Mail und Passwort eingeben."; //TODO: Localize
            return;
        }

        IsLoading = true;
        StateHasChanged();
        ErrorMessage = "";

        LoginRequest loginRequest = new
        (
            Email: Email,
            Password: Password,
            DeviceId: deviceInfoService.GetDeviceId(),
            DeviceName: deviceInfoService.GetDeviceName(),
            Platform: deviceInfoService.GetPlatform()
        );

        string loginUrl = urlBuilder.AddUrl(Endpoints.AuthEndpoints.LoginEndpoint).Build();
        Result<JwtClientData, ApiError> result = await httpApiClient.PostAsync<LoginRequest, JwtClientData>(loginUrl, loginRequest);
        if (result.IsOperationSuccess)
        {
            ErrorMessage = "true";
            //Save Jwt
            //Navigate to /home
        }
        else
        {
            ErrorMessage = $"Fehler beim Anmelden: {result.Error.ErrorCode}"; //TODO: Localize
        }

        IsLoading = false;
        StateHasChanged();
    }
}