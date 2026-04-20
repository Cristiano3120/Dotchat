using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using Microsoft.VSDiagnostics;
using RazorEngineCore;
using RazorEngine = RazorEngineCore.RazorEngine;

namespace DotchatBenchmark;

[CPUUsageDiagnoser]
public class Benchmarks
{
    public record VerificationEmailModel(
    string Name,
    string AppName,
    string ConfirmUrl,
    DateTime ExpiresAt,
    string Language = "de");

    private RazorEngine _razorEngine;
    private VerificationEmailModel _model;
    private string _templateName;

    private readonly ConcurrentDictionary<string, IRazorEngineCompiledTemplate<RazorEngineTemplateBase<VerificationEmailModel>>> _templateCaches = [];
    private readonly ConcurrentDictionary<string, DateTime> _lastCacheUpdates = [];
    private readonly ConcurrentDictionary<string, SemaphoreSlim> _locks = [];

    [GlobalSetup]
    public void Setup()
    {
        _razorEngine = new RazorEngine();
        string templateContent = File.ReadAllText("C:\\Users\\Crist\\source\\repos\\DotchatServer\\Dotchat\\src\\EmailTemplates\\De\\VerificationEmailTemplate.cshtml"); //CHANGE THIS
        _model = new VerificationEmailModel("Name", "AppName", "ConfirmUrl", DateTime.UtcNow + TimeSpan.FromHours(1));
        IRazorEngineCompiledTemplate<RazorEngineTemplateBase<VerificationEmailModel>> template = _razorEngine.Compile<RazorEngineTemplateBase<VerificationEmailModel>>(
            templateContent, builder =>
            {
                builder.AddUsing("System");
            });
        _templateName = "VerificationEmailTemplate";
        _lastCacheUpdates[_templateName] = DateTime.UtcNow;
        _templateCaches[_templateName] = template;
    }

    [Benchmark]
    public async Task CreateAsync()
    {
        string cacheKey = $"De_{_templateName}";
        string templatePath = "C:\\Users\\Crist\\source\\repos\\DotchatServer\\Dotchat\\src\\EmailTemplates\\De\\VerificationEmailTemplate.cshtml";

        SemaphoreSlim semaphore = _locks.GetOrAdd(cacheKey, _ => new SemaphoreSlim(1, 1));

        DateTime lastWriteTime = File.GetLastWriteTimeUtc(templatePath);
        bool cacheValid = _lastCacheUpdates.TryGetValue(cacheKey, out DateTime cachedTime) && cachedTime >= lastWriteTime;

        if (!cacheValid)
        {
            await semaphore.WaitAsync();
            try
            {
                // Double-check nach dem Lock
                lastWriteTime = File.GetLastWriteTimeUtc(templatePath);
                cacheValid = _lastCacheUpdates.TryGetValue(cacheKey, out cachedTime) && cachedTime >= lastWriteTime;

                if (!cacheValid)
                {
                    string templateContent = await File.ReadAllTextAsync(templatePath);
                    IRazorEngineCompiledTemplate<RazorEngineTemplateBase<VerificationEmailModel>> compiled = await _razorEngine.CompileAsync<RazorEngineTemplateBase<VerificationEmailModel>>(
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

        IRazorEngineCompiledTemplate<RazorEngineTemplateBase<VerificationEmailModel>> template = _templateCaches[cacheKey];
        string htmlBody = await template.RunAsync(instance => instance.Model = _model);
    }
}