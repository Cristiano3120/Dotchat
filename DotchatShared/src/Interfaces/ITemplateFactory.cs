using DotchatShared.src.DTOs;
using RazorEngineCore;

namespace DotchatShared.src.Interfaces;

public interface ITemplateFactory
{

    /// <summary>
    /// Creates an email message based on the specified template, model, and language. The template will be rendered with the provided model data and localized according to the specified language.
    /// </summary>
    /// <param name="templateName">The name of the email template to use. 
    /// <param name="model"></param>
    /// <returns></returns>
    Task<RawHtmlTemplate> CreateAsync<TModel>(string templateName, TModel model)
        where TModel : ITemplateNecessities;

    /// <summary>
    /// Compiles the specified template. This means that the template will be 'filled' with the provided model data
    /// </summary>
    /// <typeparam name="TModel"></typeparam>
    /// <param name="templateName"></param>
    /// <param name="language"></param>
    /// <returns></returns>
    ValueTask<IRazorEngineCompiledTemplate> CompileAsync(string templateName, string language);
}
