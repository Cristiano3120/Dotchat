using Platform = DotchatShared.src.Enums.Platform;

namespace DotchatClient.src.Application.Interfaces;

internal interface IDeviceInfoService
{
    /// <summary>
    /// Retrieves the deviceID of the current device. If the deviceID is not available, it will generate a new one and store it in the local storage for future use.
    /// </summary>
    /// <returns></returns>
    public Guid GetDeviceId();
    public string GetDeviceName();
    public Platform GetPlatform();
}