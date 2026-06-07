namespace DotchatServer.src.Core.Templates;

/// <summary>
/// A static class that contains constants for template names used in the application, such as email templates and HTML templates.
/// </summary>
public static class Templates
{
    public static class EmailTemplates
    {
        public const string VerificationEmail = "VerificationEmail";
    }

    public static class HtmlTemplates
    {
        public const string EmailConfirmed = "EmailConfirmed";
        public const string EmailConfirmationFailed = "EmailConfirmationInvalid";
        public const string ResendConfirmation = "ResendConfirmationEmailSuccessful";
        public const string EmailConfirmationFailedServerError = "EmailConfirmationFailedServerError";
    }
}