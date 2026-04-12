using DotchatServer.src.Application.DTOs.EmailModels;
using DotchatServer.src.Application.DTOs.Emails;
using DotchatServer.src.Application.Interfaces;
using DotchatShared.src.Enums;
using Microsoft.AspNetCore.Razor.Language;
using RazorEngineCore;

namespace DotchatServer.src.Infrastructure;

public sealed class EmailFactory(IRazorEngine razorEngine) : IEmailFactory
{
    public async Task<Email> CreateAsync<TModel>(string templateName, TModel model, Language language)
    {
        //TODO: Rework VerificationEmailModel/Template
        string templatePath = $"EmailTemplates/{language}/{templateName}.cshtml"; //Hardcoded for now | TODO: DONT HARDCODE
        string templateContent = await File.ReadAllTextAsync("C:\\Users\\Crist\\source\\repos\\DotchatServer\\Dotchat\\src\\EmailTemplates\\De\\VerificationEmailTemplate.cshtml"); //CHANGE THIS

        IRazorEngineCompiledTemplate<RazorEngineTemplateBase<TModel>> template = await razorEngine.CompileAsync<RazorEngineTemplateBase<TModel>>(
        templateContent, builder =>
        {
            builder.AddUsing("System");
        });

        string htmlBody = await template.RunAsync(instance => instance.Model = model); //Maybe cache the compiled template for better performance?
        return new Email(templateName, htmlBody);
    }
}