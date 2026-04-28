namespace DotchatShared.src.Constants;

public static class Endpoints
{
    public const string Base = "api";

    public static class AuthEndpoints
    {
        public const string BaseAuth = "auth";
        public const string Register = "register";
        public const string Login = "login";
        public const string Verify = "verify";
        public const string RequestVerification = "request-verification";
        public const string ConfirmEmail = "confirm-email";

        public static string BaseAuthEndpoint => $"{Base}/{BaseAuth}";
        public static string RegisterEndpoint => $"{BaseAuthEndpoint}/{Register}";
        public static string LoginEndpoint => $"{BaseAuthEndpoint}/{Login}";
        public static string VerifyEndpoint => $"{BaseAuthEndpoint}/{Verify}";
        public static string RequestVerificationEndpoint => $"{BaseAuthEndpoint}/{RequestVerification}";
        public static string ConfirmEmailEndpoint => $"{BaseAuthEndpoint}/{ConfirmEmail}";
    }
}