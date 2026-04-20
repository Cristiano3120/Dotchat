namespace DotchatServer.src.Infrastructure;

public sealed class AppPath
{
    private string _currentAppPath;
    private readonly string _root;

    private AppPath(string root)
    {
        _currentAppPath = root;
        _root = root;
    }

    public static AppPath From(IWebHostEnvironment env)
        => new(env.ContentRootPath);

    public static AppPath From(string root)
        => new(root);

    public AppPath Src()
        => Navigate("src");

    public AppPath Infrastructure()
        => Navigate("Infrastructure");

    public AppPath Application()
        => Navigate("Application");

    public AppPath Core()
        => Navigate("Core");

    public AppPath Go(string folder)
        => Navigate(folder);

    private AppPath Navigate(string folder)
    {
        _currentAppPath = Path.Combine(_currentAppPath, folder);
        return this;
    }

    public string File(string fileName)
    {
        string completePath = Path.Combine(_currentAppPath, fileName);
        _currentAppPath = _root; // Reset to root after getting the file path

        return completePath;
    }

    public override string ToString() => _currentAppPath == string.Empty
        ? _root
        : _currentAppPath;
}