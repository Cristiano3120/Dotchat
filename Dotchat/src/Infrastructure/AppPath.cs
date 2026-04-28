using DotchatServer.src.Core;

namespace DotchatServer.src.Infrastructure;

public sealed class AppPath : NavigableBase<AppPath>
{
    private AppPath(string root, string currentAppPath) : base(root, currentAppPath) { }
    private AppPath(string root) : base(root, root) { }

    public static AppPath From(IWebHostEnvironment env) => new(env.ContentRootPath);
    public static AppPath From(string root) => new(root);

    protected override AppPath Create(string root, string currentAppPath) => new(_root, currentAppPath);

    public string File(string fileName) => Path.Combine(_currentAppPath, fileName);

    public override string ToString() => _currentAppPath;
}