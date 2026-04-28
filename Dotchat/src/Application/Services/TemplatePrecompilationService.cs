using System.Diagnostics;
using System.Reflection;
using DotchatServer.src.Application.DTOs.EmailModels;
using DotchatServer.src.Application.Interfaces;
using DotchatServer.src.Core.Templates;
using DotchatServer.src.Infrastructure;
using DotchatShared.src.Enums;
using Serilog;

namespace DotchatServer.src.Application.Services;

public sealed class TemplatePrecompilationService(IEmailFactory emailFactory) : IWarmable
{
    private static IEnumerable<(string Name, Type ModelType)> GetAllTemplates() =>
    [
        (Templates.EmailTemplates.VerificationEmail, typeof(VerificationEmailModel)),
    ];

    public async Task WarmupAsync()
    {
        (Task Task, string Name, Language Language)[] started = [];
        Stopwatch sw = Stopwatch.StartNew();

        try
        {
            Log.Information("Starting template precompilation");

            IEnumerable<(string Name, Type ModelType)> templates = GetAllTemplates();
            Language[] languages = Enum.GetValues<Language>();

            IEnumerable<(Task Task, string Name, Language Language)> tasks =
                from template in templates
                from language in languages
                select (CompileTemplateAsync(template, language), template.Name, language);

            started = [.. tasks];
            await Task.WhenAll(started.Select(t => t.Task));
        }
        catch { }
        finally
        {
            foreach ((Task task, string name, Language language) in started)
            {
                if (task.IsFaulted)
                {
                    Log.Error(task.Exception, "Failed to compile {0}[{1}]", name, language);
                }
            }

            Log.Information("Precompilation done. Took {0}ms", sw.ElapsedMilliseconds);
        }
    }


    private async Task CompileTemplateAsync((string Name, Type ModelType) template, Language language)
    {
        Stopwatch sw = Stopwatch.StartNew();

        MethodInfo method = typeof(EmailFactory)
            .GetMethod(nameof(EmailFactory.CompileAsync))!
            .MakeGenericMethod(template.ModelType);

        await (Task)method.Invoke(emailFactory, [template.Name, language])!;

        Log.Information("Compiled {0}[{1}] Took: {2}ms", template.Name, language.ToString(), sw.ElapsedMilliseconds);
    }
}
