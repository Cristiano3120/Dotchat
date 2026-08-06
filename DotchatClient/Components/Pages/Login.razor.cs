using DotchatClient.src.Application;
using DotchatClient.src.Application.Interfaces;
using DotchatClient.src.Application.Services;
using DotchatClient.src.Core.DTOs;
using DotchatShared.src.Constants;
using DotchatShared.src.DTOs;
using DotchatShared.src.DTOs.AuthRequests;
using DotchatShared.src.Interfaces;

namespace DotchatClient.Components.Pages;

public partial class Login(IHttpApiClient httpApiClient, IDeviceInfoService deviceInfoService, IUrlBuilder urlBuilder) : AuthComponentBase
{
    private async Task HandleLoginAsync()
    {
        IsLoading = true;
        StateHasChanged();

        if (!ValidateFields())
        {
            IsLoading = false;
            StateHasChanged();
            return;
        }

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

    private bool ValidateFields()
    {
        Result<Unit, string> currentValidation = AuthValidator.ValidateEmail(Email);
        if (!currentValidation.IsOperationSuccess)
        {
            ErrorMessage = currentValidation.Error;
            return false;
        }

        currentValidation = AuthValidator.ValidatePassword(Password);
        if (!currentValidation.IsOperationSuccess)
        {
            ErrorMessage = currentValidation.Error;
            return false;
        }

        return true;
    }
}