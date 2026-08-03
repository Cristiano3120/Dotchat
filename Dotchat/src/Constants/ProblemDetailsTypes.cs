namespace DotchatServer.src.Constants;

/// <summary>
/// Contains string constants representing the types of problem details that can be returned by the API. 
/// </summary>
public static class ProblemDetailsTypes
{
    public const string ValidationError = "validation-error";
    public static class Auth
    {
        public const string WrongCredentials = "auth/wrong-credentials";
        public const string DbUnavailable = "auth/db-unavailable";
        public const string EmailUsernameTaken = "auth/email-username-taken";
        public const string InvalidToken = "auth/invalid-token";
    }
}