namespace DotchatServer.src.Core.Interfaces;

/// <summary>
/// Defines the necessary properties required for template generation
/// </summary>
internal interface ITemplateNecessities
{
    /// <summary>
    /// Clients language, used for localization in templates. 
    /// This has to be a valid language code (e.g., "en", "fr", "es")
    /// </summary>
    string Language { get; init; }
    string AppName { get; init; }
}