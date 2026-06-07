using DotchatServer.src.Core.Interfaces;
using RazorEngineCore;

namespace DotchatServer.src.Application.Interfaces;

internal interface ITemplateFactory<TReturn> where TReturn : IHtmlRenderable<TReturn>
{
    /// <summary>
    /// Creates an email message based on the specified template, model, and language. The template will be rendered with the provided model data and localized according to the specified language.
    /// </summary>
    /// <param name="templateName">The name of the email template to use. <see cref="Templates.EmailTemplates"/></param>
    /// <param name="model"></param>
    /// <param name="language"></param>
    /// <returns></returns>
    Task<TReturn> CreateAsync<TModel>(string templateName, TModel model)
        where TModel : ITemplateNecessities;

    /// <summary>
    /// Compiles the specified template. This means that the template will be 'filled' with the provided model data
    /// </summary>
    /// <typeparam name="TModel"></typeparam>
    /// <param name="templateName"></param>
    /// <param name="language"></param>
    /// <returns></returns>
    ValueTask<IRazorEngineCompiledTemplate<RazorEngineTemplateBase<TModel>>> CompileAsync<TModel>(string templateName, string language)
        where TModel : ITemplateNecessities;
}
