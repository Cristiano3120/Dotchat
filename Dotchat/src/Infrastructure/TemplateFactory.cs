using System.Collections.Concurrent;
using DotchatServer.src.Application.Interfaces;
using DotchatServer.src.Application.Services;
using DotchatServer.src.Core.Interfaces;
using DotchatShared.src.Enums;
using RazorEngineCore;

namespace DotchatServer.src.Infrastructure;

/// <summary>
/// A factory for creating templates.
/// </summary>
/// <typeparam name="TReturn"></typeparam>
/// <param name="razorEngine"></param>
/// <param name="resxManager"></param>
/// <param name="appPath"></param>
/// <param name="baseFolderPath"></param>
/// <param name="factory">The factory function for creating the return type. The first parameter is the subject which is optional(only used for emails), and the second is the HTML body.</param>
public class TemplateFactory<TReturn>(
    IRazorEngine razorEngine, 
    ResxManager resxManager, 
    AppPath appPath, 
    string baseFolderPath,
    Func<string?, string, TReturn> factory) : ITemplateFactory<TReturn> where TReturn : class
{
    private readonly ConcurrentDictionary<string, object> _templateCaches = new();
    private readonly ConcurrentDictionary<string, DateTime> _lastCacheUpdates = new();
    private readonly ConcurrentDictionary<string, SemaphoreSlim> _locks = new();

    public async Task<TReturn> CreateAsync<TModel>(string templateName, TModel model, Language language)
        where TModel : ITemplateNecessities
    {
        string cacheKey = await CompileAsync<TModel>(templateName, language);

        IRazorEngineCompiledTemplate<RazorEngineTemplateBase<TModel>> template = (IRazorEngineCompiledTemplate<RazorEngineTemplateBase<TModel>>)_templateCaches[cacheKey];
        string htmlBody = await template.RunAsync(instance => instance.Model = model);

        return factory(GetSubject(templateName, language), htmlBody);
    }

    public async Task<string> CompileAsync<TModel>(string templateName, Language language)
        where TModel : ITemplateNecessities
    {
        string cacheKey = $"{language}_{templateName}";
        string templatePath = appPath.Src().Go(baseFolderPath).Go(language.ToString()).File($"{templateName}.cshtml");

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
                    string templateContent = await File.ReadAllTextAsync(templatePath);
                    IRazorEngineCompiledTemplate<RazorEngineTemplateBase<TModel>> compiled = await razorEngine.CompileAsync<RazorEngineTemplateBase<TModel>>(
                        templateContent, builder =>
                        {
                            builder.AddUsing("System");
                        });

                    _templateCaches[cacheKey] = compiled;
                    _lastCacheUpdates[cacheKey] = DateTime.UtcNow;
                }
            }
            finally
            {
                _ = semaphore.Release();
            }
        }

        return cacheKey;
    }

    /// <summary>
    /// TemplateName(Resx key) has to be the same as the template name
    /// </summary>
    /// <param name="templateName"></param>
    /// <param name="language"></param>
    /// <returns></returns>
    private string GetSubject(string templateName, Language language)
    {
        try
        {
            ResxManager rm = resxManager.Go(baseFolderPath).Go("Subjects").File("Subjects.resx");
            return rm.GetString(key: templateName, language: language);
        }
        catch (Exception)
        {
            return string.Empty;
        }
    }
}
