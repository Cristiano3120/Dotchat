using Microsoft.JSInterop;

namespace DotchatClient.src.Application.Services;

public sealed class Js(IJSRuntime runtime)
{
    public async Task<string> CreateBlobUrlAsync(Stream stream) 
        => await runtime.InvokeAsync<string>("createBlobUrl", new DotNetStreamReference(stream));
}