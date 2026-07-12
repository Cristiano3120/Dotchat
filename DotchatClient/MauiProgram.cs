using DotchatClient.src.Application.Interfaces;
using DotchatClient.src.Application.Services;
using DotchatShared.src.Interfaces;
using DotchatShared.src.Services;
using Microsoft.Extensions.Logging;

namespace DotchatClient;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        MauiAppBuilder builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
            });

        builder.Services.AddMauiBlazorWebView();
        builder.Services.AddHttpClient();
        builder.Services.AddSingleton<IUrlBuilder, UrlBuilder>();
        builder.Services.AddSingleton<IDeviceInfoService, DeviceInfoService>();
        builder.Services.AddSingleton<IHttpApiClient, HttpApiClient>();

#if DEBUG
        builder.Services.AddBlazorWebViewDeveloperTools();
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}
