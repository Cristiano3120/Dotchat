using DotchatServer.src.Application.Services;

namespace DotchatServer.src.Application.Interfaces;

/// <summary>
/// Interface for building URLs in a flexible and fluent manner. The Singletion pattern is used as a starting point
/// </summary>
public interface IUrlBuilder
{
    /// <summary>
    /// Adds a base URL to the builder. This is the initial URL that will be used as the foundation for building the final URL.
    /// </summary>
    /// <returns>A UrlBuilder instance which allows for method chaining.</returns>
    public UrlBuilder AddUrl(string url);

    /// <summary>
    /// Adds a route parameter to the URL.
    /// This is used to append optional params
    /// </summary>
    /// <returns>A UrlBuilder instance which allows for method chaining.</returns>
    public UrlBuilder AddRouteParam(string value);

    /// <summary>
    /// Adds a query parameter to the URL.
    /// </summary>
    /// <returns>A UrlBuilder instance which allows for method chaining.</returns>
    public UrlBuilder AddQueryParam(string key, string value);

    /// <summary>
    /// Builds the final URL.
    /// </summary>
    /// <returns>The constructed URL.</returns>
    public string Build();
}