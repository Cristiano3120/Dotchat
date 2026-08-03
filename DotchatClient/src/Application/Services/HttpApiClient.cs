using DotchatClient.src.Application.Interfaces;
using DotchatClient.src.Core.DTOs;
using DotchatShared.src.Constants;
using DotchatShared.src.DTOs;
using DotchatShared.src.Enums;
using Serilog;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace DotchatClient.src.Application.Services;

internal sealed class HttpApiClient(IJwtTokenStorage jwtTokenStorage, HttpClient httpClient) : IHttpApiClient
{
    public async Task<Result<T, ApiError>> GetAsync<T>(string absoluteUrl, CancellationToken cancellationToken = default)
        => await SendAndReceiveAsync<T>(new HttpRequestMessage(HttpMethod.Get, absoluteUrl), cancellationToken);

    public async Task<Result<TReturn, ApiError>> PostAsync<TParam, TReturn>(string absoluteUrl, TParam data, CancellationToken cancellationToken = default)
        => await SendAndReceiveAsync<TReturn>(new HttpRequestMessage()
        {
            Content = JsonContent.Create(data),
            Method = HttpMethod.Post,
            RequestUri = new Uri(absoluteUrl, UriKind.Absolute)
        }, cancellationToken); 

    public async Task<Result<Unit, ApiError>> PostAsync<TParam>(string absoluteUrl, TParam data, CancellationToken cancellationToken = default)
        => await SendAsync(new HttpRequestMessage()
        {
            Content = JsonContent.Create(data),
            Method = HttpMethod.Post,
            RequestUri = new Uri(absoluteUrl, UriKind.Absolute)
        }, cancellationToken);

    private async Task<Result<T, ApiError>> SendAndReceiveAsync<T>(HttpRequestMessage request, CancellationToken cts) 
        => await ExecuteRequestAsync<T>(request, cts);

    private async Task<Result<Unit, ApiError>> SendAsync(HttpRequestMessage request, CancellationToken cts)
        => await ExecuteRequestAsync<Unit>(request, cts);

    private async Task<Result<T, ApiError>> ExecuteRequestAsync<T>(HttpRequestMessage request, CancellationToken cts)
    {
        try
        {
            Log.Debug("[{Method}]: {RelativeUrl}", request.Method, request.RequestUri?.ToString());
            await SetBearerTokenAsync(request);

            HttpResponseMessage response = await httpClient.SendAsync(request, cts);
            if (response.IsSuccessStatusCode)
            {
                if (typeof(T) == typeof(Unit))
                {
                    return Result<T, ApiError>.Success((T)(object)Unit.Value);
                }
                
                T? data = (T?)await response.Content.ReadFromJsonAsync(typeof(T), AppJsonContext.Default, cts);
                if (data is not null)
                {
                    return Result<T, ApiError>.Success(data);
                }
            }
            
            ApiError apiError = await DeserializeApiErrorAsync(response, cts);
            Log.Debug("API Error on {Method} {RelativeUrl}: {@ApiError}", request.Method, request.RequestUri?.ToString(), apiError);
            return Result<T, ApiError>.Failure(apiError);
        }
        catch (HttpRequestException ex)
        {
            Log.Warning(ex, "Network error on {Method} {RelativeUrl}", request.Method, request.RequestUri?.ToString());
            return Result<T, ApiError>.Failure(new ApiError
            {
                HttpStatusCode = 0,
                ErrorCode = ApiErrorCode.NetworkUnavailable,
                Title = "Connection unavailable" //TODO: Localize this
            });
        }
        catch (TaskCanceledException) when (cts.IsCancellationRequested)
        {
            // The operation was canceled by the caller, rethrow the exception to propagate it
            throw;
        }
        catch (TaskCanceledException ex)
        {
            Log.Warning(ex, "Timeout on {Method} {RelativeUrl}", request.Method, request.RequestUri?.ToString());
            return Result<T, ApiError>.Failure(new ApiError
            {
                HttpStatusCode = 0,
                ErrorCode = ApiErrorCode.Timeout,
                Title = "Timeout: Server not responding" //TODO: Localize this
            });
        }
        catch (JsonException ex)
        {
            Log.Warning(ex, "Deserialization failed on {Method} {RelativeUrl}", request.Method, request.RequestUri?.ToString());
            return Result<T, ApiError>.Failure(new ApiError
            {
                HttpStatusCode = 0,
                ErrorCode = ApiErrorCode.DeserializationError,
                Title = "Invalid response from server or wrong type expected" //TODO: Localize this
            });
        }
    }

    private async Task<ApiError> DeserializeApiErrorAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        ProblemDetails? problemDetails = (ProblemDetails?)await response.Content.ReadFromJsonAsync(typeof(ProblemDetails), AppJsonContext.Default, cancellationToken);
        if (problemDetails is null)
        {
            Log.Warning("Failed to deserialize ProblemDetails from response. StatusCode: {StatusCode}", response.StatusCode);
            return new ApiError
            {
                HttpStatusCode = response.StatusCode,
                ErrorCode = ApiErrorCode.SameAsStatusCode,
                Title = "Unknown error occurred" //TODO: Localize this
            };
        }

        ApiErrorCode errorCode = ApiErrorCode.SameAsStatusCode;
        if (problemDetails?.Extensions?.TryGetValue(ProblemDetailsExtensions.ApiErrorCode, out JsonElement errorCodeValue) ?? false)
        {
            errorCode = errorCodeValue.Deserialize<ApiErrorCode>();
        }

        ApiError error = new()
        {
            HttpStatusCode = problemDetails?.Status ?? response.StatusCode,
            ErrorCode = errorCode,
            Title = problemDetails?.Title ?? string.Empty
        };

        return error;
    }

    /// <summary>
    /// Will request a new AccessToken if needed
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    private async Task SetBearerTokenAsync(HttpRequestMessage request)
    {
        (string? accessToken, bool requestNewToken) = jwtTokenStorage.GetAccessToken();
        if (requestNewToken)
        {
            //req token
        }

        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
    }
}