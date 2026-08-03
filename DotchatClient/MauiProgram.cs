using DotchatClient.src.Application.Interfaces;
using DotchatClient.src.Application.Services;
using DotchatClient.src.Core.Enums;
using DotchatShared.src.Interfaces;
using DotchatShared.src.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace DotchatClient;

public static class MauiProgram
{
    public async static Task<MauiApp> CreateMauiAppAsync()
    {
        MauiAppBuilder builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
            });
        
        using Stream stream = await FileSystem.OpenAppPackageFileAsync("wwwroot\\appsettings.json"); 
        builder.Configuration.AddJsonStream(stream);
        _ = builder.Services.AddMauiBlazorWebView();
        
        string address = builder.Configuration.GetValue<string>("WebAddress") ?? throw new InvalidOperationException("Missing base WebAddress");
        _ = builder.Services.AddHttpClient("Api", configureClient: client => 
        {
            client.BaseAddress = new Uri(address);
        });

        _ = builder.Services.AddSingleton<IJwtTokenStorage, JwtTokenStorage>();
        _ = builder.Services.AddSingleton<IUrlBuilder, UrlBuilder>((services) => UrlBuilder.Create(address));
        _ = builder.Services.AddKeyedSingleton<AppPath>(WWWRootFolder.Base, (_, _) => AppPath.From("wwwroot"));
        _ = builder.Services.AddSingleton<IDeviceInfoService, DeviceInfoService>();
        _ = builder.Services.AddSingleton<IHttpApiClient, HttpApiClient>();
        _ = builder.Services.AddSingleton<Js>();

#if DEBUG
        _ = builder.Services.AddBlazorWebViewDeveloperTools();
        _ = builder.Logging.AddDebug();
#endif
        return builder.Build();
    }
}
