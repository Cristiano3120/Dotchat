using DotchatClient.src.Core.DTOs;
using DotchatShared.src.DTOs;

namespace DotchatClient.src.Application.Interfaces;

public interface IHttpApiClient
{
    Task<Result<T, ApiError>> GetAsync<T>(string relativeUrl, CancellationToken cancellationToken = default);
    Task<Result<TReturn, ApiError>> PostAsync<TParam, TReturn>(string relativeUrl, TParam data, CancellationToken cancellationToken = default);
    Task<Result<Unit, ApiError>> PostAsync<TParam>(string relativeUrl, TParam data, CancellationToken cancellationToken = default);
}