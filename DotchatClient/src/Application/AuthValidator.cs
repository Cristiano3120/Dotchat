using DotchatClient.src.Core.DTOs;
using DotchatShared.src.Constants;
using DotchatShared.src.DTOs;
using System.Net.Mail;
using System.Text.RegularExpressions;

namespace DotchatClient.src.Application;

internal static partial class AuthValidator
{
    [GeneratedRegex(@"^[\x21-\x7E]+$")]
    private static partial Regex PasswordRegex();

    internal static Result<Unit, string> ValidateEmail(string email)
    {
        try
        {
            //Checks for format and empty
            MailAddress addr = new(email);
            return addr.Address == email
                ? new Unit()
                : "Ungültige Email-Adresse.";
        }
        catch
        {
            return "Ungültige Email-Adresse.";
        }
    }
    
    internal static Result<Unit, string> ValidatePassword(string password)
    {
        if (string.IsNullOrWhiteSpace(password))
        {
            return "Passwort darf nicht leer sein.";
        }

        if (password.Length < AuthRequestRules.MinPasswordLength)
        {
            return $"Passwort muss mindestens {AuthRequestRules.MinPasswordLength} Zeichen lang sein.";
        }

        if (password.Length > AuthRequestRules.MaxPasswordLength)
        {
            return $"Passwort darf höchstens {AuthRequestRules.MaxPasswordLength} Zeichen lang sein.";
        }

        if (!PasswordRegex().IsMatch(password))
        {
            return "Passwort enthält ungültige Zeichen. Nur Buchstaben, Zahlen und normale Sonderzeichen sind erlaubt. Leerzeichen sind nicht erlaubt.";
        }

        return new Unit();
    }

    internal static Result<Unit, string> ValidateUsername(string username)
    {
        if (string.IsNullOrWhiteSpace(username))
        {
            return "Benutzername darf nicht leer sein.";
        }

        if (username.Length < AuthRequestRules.MinUsernameLength)
        {
            return $"Benutzername muss mindestens {AuthRequestRules.MinUsernameLength} Zeichen lang sein.";
        }

        if (username.Length > AuthRequestRules.MaxUsernameLength)
        {
            return $"Benutzername darf höchstens {AuthRequestRules.MaxUsernameLength} Zeichen lang sein.";
        }

        return new Unit();
    }

    internal static Result<Unit, string> ValidateDisplayName(string displayName)
    {
        if (string.IsNullOrWhiteSpace(displayName))
        {
            return "Anzeigename darf nicht leer sein.";
        }

        if (displayName.Length < AuthRequestRules.MinDisplayNameLength)
        {
            return $"Anzeigename muss mindestens {AuthRequestRules.MinDisplayNameLength} Zeichen lang sein.";
        }

        if (displayName.Length > AuthRequestRules.MaxDisplayNameLength)
        {
            return $"Anzeigename darf höchstens {AuthRequestRules.MaxDisplayNameLength} Zeichen lang sein.";
        }

        return new Unit();
    }

    internal static Result<Unit, string> ValidateBio(string bio)
    {
        if (bio.Length > AuthRequestRules.MaxBioLength)
        {
            return $"Bio darf höchstens {AuthRequestRules.MaxBioLength} Zeichen lang sein.";
        }

        return new Unit();
    }

    internal static Result<Unit, string> ValidateBirthday(DateOnly? birthday)
    {
        if (!birthday.HasValue)
        {
            return "Geburtsdatum darf nicht leer sein.";
        }

        if (birthday.Value > DateOnly.FromDateTime(DateTime.UtcNow))
        {
            return "Geburtsdatum darf nicht in der Zukunft liegen.";
        }

        return new Unit();
    }
}