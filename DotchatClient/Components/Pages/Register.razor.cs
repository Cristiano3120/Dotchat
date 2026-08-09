using DotchatClient.src.Application;
using DotchatClient.src.Application.Interfaces;
using DotchatClient.src.Application.Services;
using DotchatClient.src.Core;
using DotchatClient.src.Core.DTOs;
using DotchatClient.src.Core.Enums;
using DotchatShared.src.Constants;
using DotchatShared.src.DTOs;
using DotchatShared.src.DTOs.AuthRequests;
using DotchatShared.src.Interfaces;
using DotchatShared.src.Services;
using Microsoft.AspNetCore.Components;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;

namespace DotchatClient.Components.Pages;

public partial class Register(IHttpApiClient httpApiClient, IDeviceInfoService deviceInfoService, IUrlBuilder urlBuilder,
    [FromKeyedServices(WWWRootFolder.Base)] AppPath appPath, Js js) : AuthComponentBase
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
    private DateOnly? Birthday;

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
            return AtLimitClass;
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

    private async void HandleGoogleOAuth2Async()
    {
        string clientSecretUri = appPath.Go("Secrets").File("client_secret.json");
        using FileStream clientSecretsStream = new(clientSecretUri, FileMode.Open, FileAccess.Read);
        GoogleClientSecret? googleClientSecret = JsonSerializer.Deserialize<GoogleClientSecret>(clientSecretsStream);

        if (googleClientSecret is null)
        {
            ErrorMessage = "Fehler beim Laden der Google Client Secrets. Reinstallieren Sie die Anwendung."; //TODO: Localize
            return;
        }

        string scopes = string.Join(" ", GoogleScopes.GetAllScopes());
        string state = Guid.NewGuid().ToString("N");

        // Get a free port for the local HTTP listener to receive the OAuth2 redirect
        TcpListener tcpListener = new(IPAddress.Loopback, 0);
        tcpListener.Start();
        int port = ((IPEndPoint)tcpListener.LocalEndpoint).Port;
        tcpListener.Stop();

        string redirectUri = $"http://127.0.0.1:{port}/";
        string authUrl = $"https://accounts.google.com/o/oauth2/v2/auth" +
            $"?client_id={Uri.EscapeDataString(googleClientSecret.ClientId)}" +
            $"&redirect_uri={redirectUri}" +
            $"&response_type=code" +
            $"&scope={Uri.EscapeDataString(scopes)}" +
            $"&state={state}" +
            $"&access_type=online";

        HttpListener httpListener = new();
        httpListener.Prefixes.Add(redirectUri);
        httpListener.Start();
        _ = await Launcher.OpenAsync(authUrl);
        HttpListenerContext context = await httpListener.GetContextAsync(); // Wait for the OAuth2 redirect

        //TODO: pack alles in einzelne methods und pack HTML in ne file
        string? code = context.Request.QueryString["code"];
        string? returnedState = context.Request.QueryString["state"];

        string html = BuildResponseHtml(success: code is not null);
        byte[] buffer = Encoding.UTF8.GetBytes(html);
        context.Response.ContentType = "text/html; charset=utf-8";
        context.Response.ContentLength64 = buffer.Length;
        await context.Response.OutputStream.WriteAsync(buffer);
        context.Response.OutputStream.Close();

        if (returnedState != state)
        {
            ErrorMessage = "Ungültiger OAuth2-Redirect. Try again.";
            return;
        }

        if (string.IsNullOrEmpty(code))
        {
            ErrorMessage = "Fehler beim Abrufen des OAuth2-Codes. Try again.";
            return;
        }

        httpListener.Stop();
    }

    private string BuildResponseHtml(bool success)
    {
        string title = success ? "Login erfolgreich" : "Login fehlgeschlagen";
        string message = success
            ? "Du kannst dieses Fenster jetzt schließen und zu Dotchat zurückkehren."
            : "Etwas ist schiefgelaufen. Schließe dieses Fenster und versuch es erneut.";

        string logoDataUri = LoadLogoAsDataUri();

        return $$"""
    <!DOCTYPE html>
    <html lang="de">
    <head>
        <meta charset="utf-8">
        <title>Dotchat</title>
        <style>
            * {
                box-sizing: border-box;
            }

            body {
                font-family: -apple-system, BlinkMacSystemFont, "Segoe UI", sans-serif;
                background: linear-gradient(160deg, #0f0f1a 0%, #1a1030 100%);
                color: #e5e5f0;
                display: flex;
                align-items: center;
                justify-content: center;
                height: 100vh;
                margin: 0;
            }

            .card {
                background: rgba(255, 255, 255, 0.03);
                border: 1px solid rgba(139, 92, 246, 0.2);
                border-radius: 16px;
                padding: 40px;
                text-align: center;
                max-width: 360px;
                box-shadow: 0 20px 60px rgba(0, 0, 0, 0.4);
                animation: fadeIn 0.4s ease-out;
            }

            .logo {
                display: flex;
                align-items: center;
                justify-content: center;
                gap: 10px;
                margin-bottom: 32px;
            }

            .logo-icon {
                width: 36px;
                height: 36px;
                background: linear-gradient(135deg, #7c6af7, #6a59e0);
                border-radius: 10px;
                display: flex;
                align-items: center;
                justify-content: center;
                box-shadow: 0 4px 14px rgba(124, 106, 247, 0.35);
                flex-shrink: 0;
            }

            .logo-img {
                width: 20px;
                height: 20px;
                filter: brightness(0) invert(1);
                display: block;
            }

            .logo-text {
                color: #fff;
                font-size: 19px;
                font-weight: 600;
                letter-spacing: -0.3px;
            }

            .status-icon {
                width: 48px;
                height: 48px;
                border-radius: 50%;
                display: flex;
                align-items: center;
                justify-content: center;
                margin: 0 auto 20px;
                background: {{(success ? "rgba(139, 92, 246, 0.15)" : "rgba(239, 68, 68, 0.15)")}};
            }

            .status-icon svg {
                width: 24px;
                height: 24px;
                stroke: {{(success ? "#8b5cf6" : "#ef4444")}};
            }

            h2 {
                margin: 0 0 8px;
                font-size: 20px;
                font-weight: 600;
                color: {{(success ? "#c4b5fd" : "#fca5a5")}};
            }

            p {
                margin: 0;
                font-size: 14px;
                line-height: 1.5;
                color: #a1a1b5;
            }

            @keyframes fadeIn {
                from { opacity: 0; transform: translateY(8px); }
                to { opacity: 1; transform: translateY(0); }
            }
        </style>
    </head>
    <body>
        <div class="card">
            <div class="logo">
                <div class="logo-icon">
                    <img class="logo-img" src="{{logoDataUri}}" alt="Dotchat" />
                </div>
                <span class="logo-text">Dotchat</span>
            </div>
            <div class="status-icon">
                {{(success
                        ? """<svg viewBox="0 0 24 24" fill="none" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round"><polyline points="20 6 9 17 4 12"/></svg>"""
                        : """<svg viewBox="0 0 24 24" fill="none" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round"><line x1="18" y1="6" x2="6" y2="18"/><line x1="6" y1="6" x2="18" y2="18"/></svg>""")}}
            </div>
            <h2>{{title}}</h2>
            <p>{{message}}</p>
        </div>
    </body>
    </html>
    """;
    }

    private string LoadLogoAsDataUri()
    {
        try
        {
            byte[] svgBytes = File.ReadAllBytes(appPath.File(DotchatSvg));
            string base64 = Convert.ToBase64String(svgBytes);
            return $"data:image/svg+xml;base64,{base64}";
        }
        catch
        {
            return string.Empty;
        }
    }

    private async Task HandleRegisterAsync()
    {
        ToggleLoadingState();

        if (!ValidateFields())
        {
            ToggleLoadingState();
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

        ToggleLoadingState();
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