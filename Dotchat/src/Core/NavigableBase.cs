namespace DotchatServer.src.Core;

public abstract class NavigableBase<T> where T : NavigableBase<T>
{
    protected readonly string _currentAppPath;
    protected readonly string _root;

    protected NavigableBase(string root, string currentAppPath)
    {
        _root = root;
        _currentAppPath = currentAppPath;
    }

    protected abstract T Create(string root, string currentAppPath);

    public T Src() => Navigate("src");
    public T Infrastructure() => Navigate("Infrastructure");
    public T Application() => Navigate("Application");
    public T Core() => Navigate("Core");
    public T Go(string folder) => Navigate(folder);

    private T Navigate(string folder)
        => Create(_root, Path.Combine(_currentAppPath, folder));
}