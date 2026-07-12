using Platform = DotchatShared.src.Enums.Platform;
using DotchatClient.src.Application.Interfaces;

namespace DotchatClient.src.Application.Services;

internal sealed class DeviceInfoService : IDeviceInfoService
{
    public Guid GetDeviceId()
    {
        string? deviceIdString = Preferences.Get("device_id", null);
        if (string.IsNullOrEmpty(deviceIdString) || !Guid.TryParse(deviceIdString, out Guid deviceId))
        {
            deviceId = Guid.NewGuid();
            Preferences.Set("device_id", deviceId.ToString());
        }

        return deviceId;
    }

    public Platform GetPlatform()
    {
        if (DeviceInfo.Platform == DevicePlatform.Android)
            return Platform.Android;
        else if (DeviceInfo.Platform == DevicePlatform.iOS)
            return Platform.iOS;
        else if (DeviceInfo.Platform == DevicePlatform.WinUI)
            return Platform.Windows;
        else if (DeviceInfo.Platform == DevicePlatform.macOS)
            return Platform.MacOS;
        else
            throw new NotSupportedException($"Unsupported platform: {DeviceInfo.Platform}");
    }

    public string GetDeviceName() => DeviceInfo.Name;

}