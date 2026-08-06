using DotchatClient.src.Application.Interfaces;
using DotchatClient.src.Application.Services;
using DotchatClient.src.Core;
using DotchatClient.src.Core.DTOs;
using DotchatShared.src.Constants;
using DotchatShared.src.DTOs;
using DotchatShared.src.DTOs.AuthRequests;
using DotchatShared.src.Interfaces;
using System.Net.Mail;

namespace DotchatClient.Components.Pages;

public partial class Login(IHttpApiClient httpApiClient, IDeviceInfoService deviceInfoService, IUrlBuilder urlBuilder, Js js)
{
    private readonly string GitHubSvg = ImagePaths.GitHubSvg;
    private readonly string EyeSvg = ImagePaths.PasswordEyeSvg;
    private readonly string ClosedEyeSvg = ImagePaths.ClosedPasswordEyeSvg;
    private readonly string GoogleSvg = ImagePaths.GoogleSvg;
    private readonly string DotchatSvg = ImagePaths.DotchatSvg;
    private const int MaxPasswordLength = LoginRequestRules.MaxPasswordLength;
    private const int MinPasswordLength = LoginRequestRules.MinPasswordLength;
    private string Email { get; set; } = string.Empty;          
    private string Password { get; set; } = string.Empty;       
    private string? ErrorMessage { get; set; } = string.Empty;
    private bool ShowPassword { get; set; }
    private bool IsLoading { get; set; }

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
        Result<Unit, string> currentValidation = ValidateEmail(Email);
        if (!currentValidation.IsOperationSuccess)
        {
            ErrorMessage = currentValidation.Error;
            return false;
        }

        currentValidation = ValidatePassword(Password);
        if (!currentValidation.IsOperationSuccess)
        {
            ErrorMessage = currentValidation.Error;
            return false;
        }

        return true;
    }

    private static Result<Unit, string> ValidateEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return "Email darf nicht leer sein.";
        }

        try
        {
            MailAddress addr = new(email);
            return addr.Address == email
                ? new Unit()
                : "Ungültige Email-Adresse.";
        }
        catch
        {
            return "Ungültige Email-Adresse.";
        }
    }

    private static Result<Unit, string> ValidatePassword(string password)
    {
        if (string.IsNullOrWhiteSpace(password))
        {
            return "Passwort darf nicht leer sein.";
        }

        if (password.Length < MinPasswordLength)
        {
            return $"Passwort muss mindestens {MinPasswordLength} Zeichen lang sein.";
        }

        if (!PasswordRegex().IsMatch(password))
        {
            return "Passwort enthält ungültige Zeichen. Nur Buchstaben, Zahlen und die Sonderzeichen _, ., -, + sind erlaubt.";
        }

        return new Unit();
    }
}