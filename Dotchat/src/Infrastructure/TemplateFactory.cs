using System.Collections.Concurrent;
using DotchatServer.src.Application.Interfaces;
using DotchatServer.src.Application.Services;
using DotchatServer.src.Core.Interfaces;
using RazorEngineCore;
using Serilog;

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
public class TemplateFactory<TReturn>: ITemplateFactory<TReturn> where TReturn : class, IHtmlRenderable<TReturn>
{
    private readonly ConcurrentDictionary<string, object> _templateCaches = new();
    private readonly ConcurrentDictionary<string, DateTime> _lastCacheUpdates = new();
    private readonly ConcurrentDictionary<string, SemaphoreSlim> _locks = new();

    private readonly Func<string?, string, TReturn> _factory;
    private readonly IRazorEngine _razorEngine;
    private readonly ResxManager _resxManager;
    private readonly AppPath _appPath;

    private readonly string _pathToTemplates;

    public TemplateFactory(IRazorEngine razorEngine, ResxManager resxManager, AppPath appPath, Func<string?, string, TReturn> factory)
    {
        _razorEngine = razorEngine;
        _resxManager = resxManager;
        _appPath = appPath;
        _factory = factory;

        _pathToTemplates = appPath.ToString();
    }

    public async Task<TReturn> CreateAsync<TModel>(string templateName, TModel model)
        where TModel : ITemplateNecessities
    {
        IRazorEngineCompiledTemplate<RazorEngineTemplateBase<TModel>> template = await CompileAsync<TModel>(templateName, model.Language);
        string htmlBody = await template.RunAsync(instance => instance.Model = model);

        return _factory(GetSubject(templateName, model.Language), htmlBody);
    }

    public async ValueTask<IRazorEngineCompiledTemplate<RazorEngineTemplateBase<TModel>>> CompileAsync<TModel>(string templateName, string language)
        where TModel : ITemplateNecessities
    {
        string cacheKey = $"{language}_{templateName}";
        string templatePath = _appPath.Go(_pathToTemplates).Go(language.ToString()).File($"{templateName}.cshtml");

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
                        string templateContent = await File.ReadAllTextAsync(templatePath);
                        IRazorEngineCompiledTemplate<RazorEngineTemplateBase<TModel>> compiled = await _razorEngine.CompileAsync<RazorEngineTemplateBase<TModel>>(
                            templateContent, builder =>
                            {
                                builder.AddUsing("System");
                            });

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

        return (IRazorEngineCompiledTemplate<RazorEngineTemplateBase<TModel>>)_templateCaches[cacheKey];
    }

    /// <summary>
    /// TemplateName(Resx key) has to be the same as the template name
    /// </summary>
    /// <param name="templateName"></param>
    /// <param name="language"></param>
    /// <returns></returns>
    private string GetSubject(string templateName, string language)
    {
        try
        {
            ResxManager rm = _resxManager.Go(_pathToTemplates).Go("Subjects").File("Subjects.resx");
            return rm.GetString(key: templateName, language);
        }
        catch (Exception)
        {
            return string.Empty;
        }
    }
}