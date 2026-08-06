using DotchatClient.src.Application;
using DotchatClient.src.Application.Interfaces;
using DotchatClient.src.Application.Services;
using DotchatClient.src.Core.Consts;
using DotchatClient.src.Core.DTOs;
using DotchatShared.src.Constants;
using DotchatShared.src.DTOs;
using DotchatShared.src.DTOs.AuthRequests;
using DotchatShared.src.Interfaces;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace DotchatClient.Components.Pages;

public partial class Register(IHttpApiClient httpApiClient, IDeviceInfoService deviceInfoService, IUrlBuilder urlBuilder, Js js) : AuthComponentBase
{
    private const int MinDisplayNameLength = AuthRequestRules.MinDisplayNameLength;
    private const int MaxDisplayNameLength = AuthRequestRules.MaxDisplayNameLength;
    private const int MinUsernameLength = AuthRequestRules.MinUsernameLength;
    private const int MaxUsernameLength = AuthRequestRules.MaxUsernameLength;
    private const int MaxBioLength = AuthRequestRules.MaxBioLength;
    private const int RecommendedPasswordLength = 16;
    private const string AtLimitClass = "at-limit";
    private const string NearLimitClass = "near-limit";
    private const string RecommendedClass = "recommended";
    private const string DefaultClass = "";

    private string Username = string.Empty;
    private string DisplayName = string.Empty;
    private string Bio = string.Empty;
    private DateOnly? Birthday = DateOnly.FromDateTime(DateTime.UtcNow);

    private readonly string _today = DateTime.UtcNow.Date.ToString("yyyy-MM-dd");
    private ElementReference _birthdayInput;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            try
            {
                await js.LimitDateYearAsync(_birthdayInput);
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
        const float NearLimitThreshold = 0.8f;
        if (current >= max)
        {
            return AtLimitClass;
        }

        return current >= max * NearLimitThreshold
            ? NearLimitClass
            : DefaultClass;
    }

    /// <summary>
    /// Calculates the state of the input count. For example if a textbox needs a minimum of 8 chars 
    /// and a recommended amount of 12 chars the color of the count will be red till 8 chars,
    /// yellow till 12 chars and green after that
    /// </summary>
    /// <param name="current"></param>
    /// <param name="min"></param>
    /// <param name="recommended"></param>
    /// <returns></returns>
    private static string GetRecommendedMinMaxCountClass(int current, int min, int recommended, int max)
    {
        if (current < min)
        {
            return AtLimitClass;
        }

        if (current < recommended)
        {
            return NearLimitClass;
        }

        const float NearLimitThreshold = 0.9f;
        if (current > max * NearLimitThreshold && current < max)
        {
            return NearLimitClass;
        }

        if (current >= max)
        {
            return AtLimitClass;
        }

        return RecommendedClass;
    }

    private static string GetMinMaxCountClass(int current, int min, int max)
    {
        if (current < min)
        {
            return AtLimitClass; //TODO: mach in ne property
        }

        if (current >= max)
        {
            return AtLimitClass;
        }

        const float NearLimitThreshold = 0.8f;
        return current >= max * NearLimitThreshold
            ? NearLimitClass
            : DefaultClass;

    }

    private async Task HandleRegisterAsync()
    {
        IsLoading = true;
        StateHasChanged();

        if (!ValidateFields())
        {
            IsLoading = false;
            StateHasChanged();
            return;
        }

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
            ErrorMessage = "True"; 
                                   //Save Jwt
                                   //Navigate to /home
            return;
        }

        if (result.Error.HttpStatusCode == System.Net.HttpStatusCode.BadRequest)
        {
            ErrorMessage = $"Fehler beim Anmelden: {result.Error.Title}";
        }
        else
        {
            ErrorMessage = $"Fehler beim Anmelden: {result.Error.Title}"; //TODO: Localize
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

        currentValidation = AuthValidator.ValidateUsername(Username);
        if (!currentValidation.IsOperationSuccess)
        {
            ErrorMessage = currentValidation.Error;
            return false;
        }

        currentValidation = AuthValidator.ValidateDisplayName(DisplayName);
        if (!currentValidation.IsOperationSuccess)
        {
            ErrorMessage = currentValidation.Error;
            return false;
        }

        currentValidation = AuthValidator.ValidateBio(Bio);
        if (!currentValidation.IsOperationSuccess)
        {
            ErrorMessage = currentValidation.Error;
            return false;
        }

        currentValidation = AuthValidator.ValidateBirthday(Birthday);
        if (!currentValidation.IsOperationSuccess)
        {
            ErrorMessage = currentValidation.Error;
            return false;
        }

        return true;
    }
}