namespace DotchatServer.src.Core.Entities;

public sealed class AppSettings
{
    public string AppName { get; init; } = default!;
    public string WebAddress { get; init; } = default!;
}