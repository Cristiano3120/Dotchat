using System.Diagnostics;
using DotchatServer.src.Application.DTOs;
using DotchatServer.src.Application.DTOs.EmailModels;
using DotchatServer.src.Application.DTOs.Emails;
using DotchatServer.src.Application.Interfaces;
using DotchatServer.src.Core.Templates;
using DotchatShared.src.Enums;
using Serilog;

namespace DotchatServer.src.Application.Services;

public sealed class TemplatePrecompilationService
    (ITemplateFactory<Email> emailFactory, 
    ITemplateFactory<ConfirmationEmailTemplate> confirmationEmailFactory,
    ITemplateFactory<ResendConfirmationEmailTemplate> resendConfirmationEmailTemplateFactory) : IWarmable
{
    private IEnumerable<(string Name, Func<Language, Task> Compile)> GetAllTemplates() =>
    [
        (Templates.EmailTemplates.VerificationEmail,
            lang => emailFactory.CompileAsync<VerificationEmailModel>(Templates.EmailTemplates.VerificationEmail, lang.ToString())),

        (Templates.HtmlTemplates.EmailConfirmed,
            lang => confirmationEmailFactory.CompileAsync<EmailConfirmationStatus>(Templates.HtmlTemplates.EmailConfirmed, lang.ToString())),

        (Templates.HtmlTemplates.EmailConfirmationFailed,
            lang => confirmationEmailFactory.CompileAsync<EmailConfirmationStatus>(Templates.HtmlTemplates.EmailConfirmationFailed, lang.ToString())),

        (Templates.HtmlTemplates.ResendConfirmation,
            lang => resendConfirmationEmailTemplateFactory.CompileAsync<ResendConfirmationEmailModel>(Templates.HtmlTemplates.ResendConfirmation, lang.ToString())),
    ];

    public async Task WarmupAsync()
    {
        Stopwatch sw = Stopwatch.StartNew();
        Log.Information("Starting template precompilation");

        (Task Task, string Name, Language Language)[] started =
            [.. GetAllTemplates().SelectMany(t => Enum.GetValues<Language>(), (t, l) => (Task: CompileTemplateAsync(t, l), t.Name, Language: l))];

        try
        {
            await Task.WhenAll(started.Select(t => t.Task));
        }
        catch (Exception)
        {
            // The individual exceptions are handled in the finally block
            //We just need to catch the ex here so the application doesn't crash
        }
        finally
        {
            foreach ((Task? task, string? name, Language language) in started.Where(t => t.Task.IsFaulted))
                Log.Error(task.Exception, "Failed to compile {0}[{1}]", name, language);

            Log.Information("Precompilation done. Took {0}ms", sw.ElapsedMilliseconds);
        }
    }


    private static async Task CompileTemplateAsync((string Name, Func<Language, Task> Compile) template, Language language)
    {
        Stopwatch sw = Stopwatch.StartNew();
        await template.Compile(language);

        Log.Information("Compiled {0}[{1}] Took: {2}ms", template.Name, language, sw.ElapsedMilliseconds);
    }
}