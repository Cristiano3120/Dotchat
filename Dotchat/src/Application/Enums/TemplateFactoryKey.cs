namespace DotchatServer.src.Application.Enums;

/// <summary>
/// Provides 'keys' for identifying different template factories.
/// These keys are used to retrieve the appropriate template factory from the dependency injection container via the KeyedService methods.
/// </summary>
internal enum TemplateFactoryKey
{
    Confirmation,
    ResendConfirmation
}