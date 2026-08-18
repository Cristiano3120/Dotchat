using System.Diagnostics;
using DotchatShared.src.Enums;
using DotchatShared.src.Interfaces;
using Serilog;

namespace DotchatShared.src.Services;

public sealed class TemplatePrecompiler(ITemplateFactory templateFactory) : IWarmable 
{
    private List<(Task Task, string Name, Language Language)>? _templates = [];
    private readonly ITemplateFactory _templateFactory = templateFactory;

    public void AddTemplatesToCompileAsync(IEnumerable<string> templateNames)
    {
        Language[] languages = Enum.GetValues<Language>();
        foreach(string templateName in templateNames)
        {
            foreach (Language language in languages)
            {
                Task compileTask = CompileAsync(templateName, language);
                _templates!.Add((compileTask, templateName, language));
            }
        }

        async Task CompileAsync(string templateName, Language lang)
        {
            Stopwatch sw = Stopwatch.StartNew();
            _ = await _templateFactory.CompileAsync(templateName, lang.ToString());
            Log.Information("Compiled {Name}[{Language}] Took: {Elapsed}ms", templateName, lang, sw.ElapsedMilliseconds);
        }
    }

    public async Task WarmupAsync()
    {
        Stopwatch sw = Stopwatch.StartNew();
        Log.Information("Starting template precompilation");

        try
        {
            await Task.WhenAll(_templates!.Select(t => t.Task));
        }
        catch (Exception)
        {
            // The individual exceptions are handled in the finally block
            // We just need to catch the ex here so the application doesn't crash
        }
        finally
        {
            foreach ((Task task, string name, Language language) in _templates!.Where(t => t.Task.IsFaulted))
            {
                Log.Error(task.Exception, "Failed to compile {Name}[{Language}]", name, language);
            }

            Log.Information("Precompilation done. Took {Elapsed}ms", sw.ElapsedMilliseconds);
            Dispose();
        }
    }

    private void Dispose()
    {
        _templates?.Clear();
        _templates = null;
    }
}