using DotchatClient.src.Core.Consts;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace DotchatClient.src.Application.Services;

public sealed class Js(IJSRuntime runtime)
{
    public async Task<string> CreateBlobUrlAsync(Stream stream) 
        => await runtime.InvokeAsync<string>(JsFuncs.CreateBlobUrl, new DotNetStreamReference(stream));

    public async Task LimitDateYearAsync(ElementReference element)
        => await runtime.InvokeVoidAsync(JsFuncs.LimitDateYear, element);
}