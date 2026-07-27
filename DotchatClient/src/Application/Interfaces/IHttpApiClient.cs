using DotchatClient.src.Core.DTOs;
using DotchatShared.src.DTOs;

namespace DotchatClient.src.Application.Interfaces;

public interface IHttpApiClient
{
    Task<Result<T, ApiError>> GetAsync<T>(string absoluteUrl, CancellationToken cancellationToken = default);
    Task<Result<TReturn, ApiError>> PostAsync<TParam, TReturn>(string absoluteUrl, TParam data, CancellationToken cancellationToken = default);
    Task<Result<Unit, ApiError>> PostAsync<TParam>(string absoluteUrl, TParam data, CancellationToken cancellationToken = default);
}