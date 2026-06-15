namespace DotchatServer.src.Constants;

/// <summary>
/// Defines constant names for HttpClient instances used in the application. 
/// These names are used when receiving HttpClient instances from the IHttpClientFactory
/// </summary>
public static class HttpClientNames
{
    public const string HealthCheckClient = "HealthCheck";
}