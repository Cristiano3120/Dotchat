using DotchatServer.src.Constants;
using DotchatShared.src.Constants;

namespace DotchatServer.src.Application.Services;

public sealed class PipelineWarmupService
{
    private readonly HttpClient _httpClient;
    public PipelineWarmupService(IHttpClientFactory httpClientFactory)
        => _httpClient = httpClientFactory.CreateClient(name: HttpClientNames.HealthCheckClient);

    public async Task WarmupAsync()
    {
        try
        {
            HttpResponseMessage? msg = await _httpClient.GetAsync(Endpoints.HealthEndpoints.Liveness);
            if (msg is null || !msg.IsSuccessStatusCode)
            {
                Log.Fatal("Pipeline warmup failed. Liveness endpoint returned status code {StatusCode}", msg?.StatusCode ?? default);
                return;
            }

            msg = await _httpClient.GetAsync(Endpoints.HealthEndpoints.Readiness);
            if (msg is null || !msg.IsSuccessStatusCode)
            {
                Log.Fatal("Pipeline warmup failed. Readiness endpoint returned status code {StatusCode}", msg?.StatusCode ?? default);
                return;
            }

            Log.Debug("Pipeline warmup completed successfully.");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Pipeline warmup failed with an exception.");
        }
    }
}
