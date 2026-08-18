using DotchatShared.src.DTOs;
using DotchatShared.src.Interfaces;
using DotchatShared.src.Services;
using RazorEngineCore;
using System.Collections.Concurrent;
using System.Text.RegularExpressions;

namespace DotchatServer.src.Infrastructure;

/// <summary>
/// A factory for creating cshtml templates.
/// </summary>
/// <param name="razorEngine"></param>
/// <param name="appPath"></param>
internal partial class TemplateFactory(IRazorEngine razorEngine, AppPath appPath) : ITemplateFactory
{
    private readonly ConcurrentDictionary<string, object> _templateCaches = new();
    private readonly ConcurrentDictionary<string, DateTime> _lastCacheUpdates = new();
    private readonly ConcurrentDictionary<string, SemaphoreSlim> _locks = new();

    private readonly IRazorEngine _razorEngine = razorEngine;
    private readonly AppPath _appPath = appPath;

    public async Task<RawHtmlTemplate> CreateAsync<TModel>(string templateName, TModel model)
        where TModel : ITemplateNecessities
    {
        IRazorEngineCompiledTemplate template = await CompileAsync(templateName, model.Language);
        string htmlBody = await template.RunAsync(model);
        return new RawHtmlTemplate(htmlBody);
    }

    public async ValueTask<IRazorEngineCompiledTemplate> CompileAsync(string templateName, string language)
    {
        string cacheKey = $"{language}_{templateName}";
        string templatePath = GetTemplatePath(templateName, language);

        SemaphoreSlim semaphore = _locks.GetOrAdd(cacheKey, _ => new SemaphoreSlim(1, 1));

        DateTime lastWriteTime = File.GetLastWriteTimeUtc(templatePath);
        bool cacheValid = _lastCacheUpdates.TryGetValue(cacheKey, out DateTime cachedTime) && cachedTime >= lastWriteTime;

        if (!cacheValid)
        {
            await semaphore.WaitAsync();
            try
            {
                // Double-check after lock
                lastWriteTime = File.GetLastWriteTimeUtc(templatePath);
                cacheValid = _lastCacheUpdates.TryGetValue(cacheKey, out cachedTime) && cachedTime >= lastWriteTime;

                if (!cacheValid)
                {
                    try
                    {
                        (Type model, string templateContent) = await ExtractModelTypeAsync(templatePath);
                        IRazorEngineCompiledTemplate compiled = await _razorEngine.CompileAsync(templateContent, builderAction: builder => builder.AddUsing("System"));

                        _templateCaches[cacheKey] = compiled;
                        _lastCacheUpdates[cacheKey] = DateTime.UtcNow;
                    }
                    catch (Exception ex) 
                    {
                        Log.Error("Error occurred while compiling template: {TemplatePath}\n{Exception}", templatePath, ex);
                        throw;
                    }
                }
            }
            finally
            {
                _ = semaphore.Release();
            }
        }

        return (IRazorEngineCompiledTemplate)_templateCaches[cacheKey];
    }

    private string GetTemplatePath(string templateName, string language) 
        => _appPath.Go(language.ToString()).File($"{templateName}.cshtml");


    [GeneratedRegex(@"^@model\s+([\w\.]+)\s*\r?", RegexOptions.Multiline | RegexOptions.Compiled)]
    private partial Regex ModelRegex();
    private async Task<(Type model, string content)> ExtractModelTypeAsync(string templatePath)
    {
        string[] lines = await File.ReadAllLinesAsync(templatePath);
        Match match = ModelRegex().Match(lines[0]);

        if (!match.Success)
        {
            throw new InvalidOperationException("Template doesnt contain an @model-Directive.");
        }

        string typeName = match.Groups[1].Value;

        Type modelType = AppDomain.CurrentDomain.GetAssemblies()
            .Select(a => a.GetType(typeName))
            .FirstOrDefault(t => t is not null)
            ?? throw new InvalidOperationException($"Model-Type '{typeName}' couldnt be parsed.");


        return (modelType, string.Join(Environment.NewLine, lines[1..]));
    }
}