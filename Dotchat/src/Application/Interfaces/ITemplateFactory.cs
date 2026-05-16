using DotchatServer.src.Core.Interfaces;

namespace DotchatServer.src.Application.Interfaces;

public interface ITemplateFactory<TReturn>
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

    Task<string> CompileAsync<TModel>(string templateName, string language)
        where TModel : ITemplateNecessities;
}
