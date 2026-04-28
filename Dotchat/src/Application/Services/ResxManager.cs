using System.Collections.Concurrent;
using System.Globalization;
using System.Resources;
using DotchatServer.src.Core;

namespace DotchatServer.src.Application.Services;

public sealed class ResxManager : NavigableBase<ResxManager>
{
    private static readonly ConcurrentDictionary<string, ResourceManager> _cache = new();

    private ResourceManager _resourceManager;
    private readonly string _assemblyRootPath; 

    private ResxManager(string root, string assemblyRootPath) : base(root, assemblyRootPath)
    {
        _assemblyRootPath = assemblyRootPath;
        _resourceManager = null!;
    }

    private ResxManager(ResourceManager resourceManager, string currentAppPath, string root, string assemblyRootPath) : base(root, currentAppPath)
    {
        _resourceManager = resourceManager;
        _assemblyRootPath = assemblyRootPath;
    }

    public static ResxManager From(IWebHostEnvironment env) => new(env.ContentRootPath, env.ContentRootPath);
    public static ResxManager From(string root) => new(root, root);

    protected override ResxManager Create(string root, string currentAppPath)
            => new(_resourceManager, currentAppPath, root, _assemblyRootPath);

    public ResxManager File(string fileName)
    {
        string filePath = Path.Combine(_currentAppPath, fileName);
        string resourceName = PathToResourceName(filePath);
        ResourceManager rm = _cache.GetOrAdd(resourceName, name => new ResourceManager(name, typeof(ResxManager).Assembly));

        return new(rm, filePath, _root, _assemblyRootPath);
    }

    public string GetString(string key, string language)
    {
        if (_resourceManager is null)
        {
            throw new InvalidOperationException($"Call {nameof(File)} before {nameof(GetString)}.");
        }

        return _resourceManager.GetString(key, new CultureInfo(language)) 
            ?? throw new KeyNotFoundException($"Key '{key}' not found.");
    }

    public ResxManager Reset() => new(_root, _assemblyRootPath);
    public bool IsRoot() => _currentAppPath == _root;

    /// <summary>
    /// Coonverts the path to a resource name.
    /// e.g. "C:/proj/src/EmailTemplates/Subjects" → "DotchatServer.src.EmailTemplates.Subjects"
    /// </summary>
    private string PathToResourceName(string absolutePath)
    {
        string assemblyName = typeof(ResxManager).Assembly.GetName().Name!;

        // Get relative path from assembly root to the file
        string relativePath = Path.GetRelativePath(_assemblyRootPath, absolutePath);

        // remove .resx extension if present 
        if (relativePath.EndsWith(".resx", StringComparison.OrdinalIgnoreCase))
            relativePath = relativePath[..^5];

        // '//' → '.'
        string resourceName = relativePath
            .Replace(Path.DirectorySeparatorChar, '.')
            .Replace(Path.AltDirectorySeparatorChar, '.');

        return $"{assemblyName}.{resourceName}";
    }
}