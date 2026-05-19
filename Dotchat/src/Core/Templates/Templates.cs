namespace DotchatServer.src.Core.Templates;

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
        public const string ResendConfirmationFailed = "ResendConfirmationEmailFailed";
    }
}