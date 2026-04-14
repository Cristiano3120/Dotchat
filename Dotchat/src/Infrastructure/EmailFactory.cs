using DotchatServer.src.Application.DTOs.Emails;
using DotchatServer.src.Application.Interfaces;
using DotchatServer.src.Core.Interfaces;
using DotchatShared.src.Enums;
using RazorEngineCore;
using System.Collections.Concurrent;

namespace DotchatServer.src.Infrastructure;

public sealed class EmailFactory(IRazorEngine razorEngine, AppPath appPath) : IEmailFactory
{
    private readonly ConcurrentDictionary<string, object> _templateCaches = new();
    private readonly ConcurrentDictionary<string, DateTime> _lastCacheUpdates = new();
    private readonly ConcurrentDictionary<string, SemaphoreSlim> _locks = new();

    public async Task<Email> CreateAsync<TModel>(string templateName, TModel model, Language language)
        where TModel : IEmailTemplateNecessities
    {
        string cacheKey = $"{language}_{templateName}";
        string templatePath = appPath.Src().Go("EmailTemplates").Go(language.ToString()).File($"{templateName}.cshtml");

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

        IRazorEngineCompiledTemplate<RazorEngineTemplateBase<TModel>> template = (IRazorEngineCompiledTemplate<RazorEngineTemplateBase<TModel>>)_templateCaches[cacheKey];
        string htmlBody = await template.RunAsync(instance => instance.Model = model);

        return new Email(model.Subject, htmlBody);
    }
}