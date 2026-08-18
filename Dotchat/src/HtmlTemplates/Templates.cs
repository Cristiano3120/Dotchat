using System.Reflection;

namespace DotchatServer.src.Core.HtmlTemplates;

/// <summary>
/// A static class that contains constants for template names used in the application, such as email templates and HTML templates.
/// </summary>
public static class Templates
{
    /// <summary>
    /// Gets all the fields(consts) of the class.
    /// </summary>
    public static IEnumerable<string> GetAllTemplateNames()
    {
        Type type = typeof(Templates);
        foreach (FieldInfo template in type.GetFields())
        {
            yield return (string)template.GetValue(type)!;
        }
    }

    public const string VerificationEmail = "VerificationEmail";
    public const string EmailConfirmed = "EmailConfirmed";
    public const string EmailConfirmationFailed = "EmailConfirmationInvalid";
    public const string ResendConfirmation = "ResendConfirmationEmailSuccessful";
    public const string EmailConfirmationFailedServerError = "EmailConfirmationFailedServerError";
}