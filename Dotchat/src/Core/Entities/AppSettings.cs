namespace DotchatServer.src.Core.Entities;

/// <summary>
/// Represents basic application information/settings.
/// <paramref name="WebAddress"/> The adress which the application is hosted on. HTTP requests will be made to this address
/// </summary>
/// <remarks>
/// These settings are retrieved from the configuration file
/// </remarks>
public sealed record AppSettings(string AppName, string WebAddress)
{
    public AppSettings() : this(AppName: string.Empty, WebAddress: string.Empty) { }
}