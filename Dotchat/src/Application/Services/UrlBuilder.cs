using DotchatServer.src.Application.Interfaces;

namespace DotchatServer.src.Application.Services;

public sealed class UrlBuilder : IUrlBuilder
{
    private readonly string _root = string.Empty;
    private readonly string _url = string.Empty;

    public UrlBuilder AddUrl(string url) => new(_root, url);

    public UrlBuilder AddRouteParam(string value)
    {
        if (string.IsNullOrEmpty(_url))
        {
            throw new InvalidOperationException("URL must be set before adding route parameters.");
        }

        // Ensure there's exactly one '/' between the existing URL and the new route parameter
        return new(_root, url: $"{_url.TrimEnd('/')}/{Uri.EscapeDataString(value.TrimStart('/'))}");
    }

    public UrlBuilder AddQueryParam(string key, string value)
    {
        if (string.IsNullOrEmpty(_url))
        {
            throw new InvalidOperationException("URL must be set before adding query parameters.");
        }

        char separator = _url.Contains('?') ? '&' : '?';
        return new(_root, url: $"{separator}{Uri.EscapeDataString(key)}={Uri.EscapeDataString(value)}");
    }

    public string Build()
    {
        if (string.IsNullOrEmpty(_url))
        {
            throw new InvalidOperationException("URL must be set before building.");
        }

        return _root + '/' + _url;
    }

    public static UrlBuilder Create(string root) => new(root, string.Empty);

    private UrlBuilder(string root, string url)
    {
        _root = root;
        _url = url;
    }
}