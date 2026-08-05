using DotchatClient.src.Application.Interfaces;
using DotchatClient.src.Core;
using DotchatClient.src.Core.DTOs;
using DotchatShared.src.Constants;
using DotchatShared.src.DTOs;
using DotchatShared.src.DTOs.AuthRequests;
using DotchatShared.src.Interfaces;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System.Net.Mail;

namespace DotchatClient.Components.Pages;

public partial class Register(IHttpApiClient httpApiClient, IDeviceInfoService deviceInfoService, IUrlBuilder urlBuilder)
{
    private const string GitHubSvg = ImagePaths.GitHubSvg;
    private const string EyeSvg = ImagePaths.PasswordEyeSvg;
    private const string ClosedEyeSvg = ImagePaths.ClosedPasswordEyeSvg;
    private const string GoogleSvg = ImagePaths.GoogleSvg;
    private const string DotchatSvg = ImagePaths.DotchatSvg;
    private const int MinDisplayNameLength = RegisterRequestRules.MinDisplayNameLength;
    private const int MaxDisplayNameLength = RegisterRequestRules.MaxDisplayNameLength;
    private const int MinUsernameLength = RegisterRequestRules.MinUsernameLength;
    private const int MinPasswordLength = RegisterRequestRules.MinPasswordLength;
    private const int MaxUsernameLength = RegisterRequestRules.MaxUsernameLength;
    private const int MaxBioLength = RegisterRequestRules.MaxBioLength;
    private const int RecommendedPasswordLength = 16;
    private const string AtLimitClass = "at-limit";
    private const string NearLimitClass = "near-limit";
    private const string RecommendedClass = "recommended";
    private const string DefaultClass = "";

    private string Email = "Cristianocx7@gmail.com"; //TODO: Mach das Password und Username und so nen min chars wert haben der dann halt am start rot ist
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
            return AtLimitClass;
        }

        return current >= max * nearLimitThreshold
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
    private static string GetRecommendedCountClass(int current, int min, int recommended)
    {
        if (current < min)
        {
            return AtLimitClass;
        }

        if (current < recommended)
        {
            return NearLimitClass;
        }

        return RecommendedClass;// Mach nen min max ding
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

        float nearLimitThreshold = 0.8f;
        return current >= max * nearLimitThreshold
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
            ErrorMessage = "True"; //TODO: Mach local field test also check ob leer/valid via regex bei email und implement 
                                   // vernünftige error messages musst validation errors die vom server kopmmen mappen
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

        currentValidation = ValidateUsername(Username);
        if (!currentValidation.IsOperationSuccess)
        {
            ErrorMessage = currentValidation.Error;
            return false;
        }

        currentValidation = ValidateDisplayName(DisplayName);
        if (!currentValidation.IsOperationSuccess)
        {
            ErrorMessage = currentValidation.Error;
            return false;
        }

        currentValidation = ValidateBio(Bio);
        if (!currentValidation.IsOperationSuccess)
        {
            ErrorMessage = currentValidation.Error;
            return false;
        }

        currentValidation = ValidateBirthday(Birthday);
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

        return new Unit();
    }

    private static Result<Unit, string> ValidateUsername(string username)
    {
        if (string.IsNullOrWhiteSpace(username))
        {
            return "Benutzername darf nicht leer sein.";
        }

        if (username.Length < MinUsernameLength)
        {
            return $"Benutzername muss mindestens {MinUsernameLength} Zeichen lang sein.";
        }

        if (username.Length > MaxUsernameLength)
        {
            return $"Benutzername darf höchstens {MaxUsernameLength} Zeichen lang sein.";
        }

        return new Unit();
    }

    private static Result<Unit, string> ValidateDisplayName(string displayName)
    {
        if (string.IsNullOrWhiteSpace(displayName))
        {
            return "Anzeigename darf nicht leer sein.";
        }

        if (displayName.Length < MinDisplayNameLength)
        {
            return $"Anzeigename muss mindestens {MinDisplayNameLength} Zeichen lang sein.";
        }

        if (displayName.Length > MaxDisplayNameLength)
        {
           return $"Anzeigename darf höchstens {MaxDisplayNameLength} Zeichen lang sein.";
        }

        return new Unit();
    }

    private static Result<Unit, string> ValidateBio(string bio)
    {
        if (bio.Length > MaxBioLength)
        {
            return $"Bio darf höchstens {MaxBioLength} Zeichen lang sein.";
        }

        return new Unit();
    }

    private static Result<Unit, string> ValidateBirthday(DateOnly? birthday)
    {
        if (!birthday.HasValue)
        {
            return "Geburtsdatum darf nicht leer sein.";
        }

        if (birthday.Value > DateOnly.FromDateTime(DateTime.UtcNow))
        {
            return "Geburtsdatum darf nicht in der Zukunft liegen.";
        }

        return new Unit();
    }
}