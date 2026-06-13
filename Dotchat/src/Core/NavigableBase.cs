namespace DotchatServer.src.Core;

/// <summary>
/// A base class for navigating through the project structure. 
/// It provides methods to navigate to common folders like "src", "Infrastructure", "Application", and "Core". 
/// It also allows navigating to any specified folder. 
/// </summary>
/// <typeparam name="T">The type of the derived class that inherits from NavigableBase.</typeparam>
/// <remarks>
/// The actual navigation logic is implemented in the derived classes by overriding the Create method.
/// </remarks>
internal abstract class NavigableBase<T> where T : NavigableBase<T>
{
    protected readonly string _currentAppPath;
    protected readonly string _root;

    protected NavigableBase(string root, string currentAppPath)
    {
        _root = root;
        _currentAppPath = currentAppPath;
    }

    protected abstract T Create(string root, string currentAppPath);

    /// <summary>
    /// Navigates to the "src" folder within the current application path.
    /// </summary>
    /// <returns></returns>
    public T Src() => Navigate("src");
    
    /// <summary>
    /// Navigates to the "Infrastructure" folder within the current application path.
    /// </summary>
    /// <returns></returns>
    public T Infrastructure() => Navigate("Infrastructure");
    
    /// <summary>
    /// Navigates to the "Application" folder within the current application path.
    /// </summary>
    /// <returns></returns>
    public T Application() => Navigate("Application");
    
    /// <summary>
    /// Navigates to the "Core" folder within the current application path.
    /// </summary>
    /// <returns></returns>
    public T Core() => Navigate("Core");

    /// <summary>
    /// Navigates to a specified folder within the current application path.
    /// </summary>
    /// <param name="folder"></param>
    /// <returns></returns>
    public T Go(string folder) => Navigate(folder);

    private T Navigate(string folder)
        => Create(_root, Path.Combine(_currentAppPath, folder));
}