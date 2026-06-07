using DotchatServer.src.Application.DTOs;
using DotchatShared.src.DTOs.AuthRequests;

namespace DotchatServer.src.Application.Interfaces;

/// <summary>
/// Service responsible for handling user registration, email confirmation, and resending verification emails.
/// </summary>
public interface IAuthService
{
    /// <summary>
    /// Registers a new user. If the registration is successful, a verification email is sent to the user and a JWT token is returned
    /// If the registration fails, a <see cref="RegisterErrorType"/> is returned with the appropriate error type
    /// </summary>
    /// <returns>
    /// The methods returns a <see cref="RegisterResult"/> which is a union that can either be a <see cref="RegisterResponse"/> or a <see cref="RegisterError"/>.
    /// </returns>
    public Task<RegisterResult> RegisterAsync(RegisterRequest registerRequest, string language);

    /// <summary>
    /// Resends a verification email to the user with the given userID. 
    /// If the user's email is already confirmed, a template indicating that is returned instead.
    /// </summary>
    /// <param name="userID">The ID of the user to resend the verification email to.</param>
    /// <param name="resendUrl">The URL to include in the verification email for resending confirmation.</param>
    /// <param name="lang">The language or culture code used to localize the returned status template.</param>
    /// <returns>An IHtmlRenderable containing the localized email confirmation status view.</returns>
    public Task<IHtmlRenderable> ResendVerificationEmailAsync(long userID, string lang);

    /// <summary>
    /// Confirms a user's email using the provided verification token and returns a localized HTML status view.
    /// </summary>
    /// <remarks>Validates the token in Redis, logs failures, invokes the authentication repository to mark
    /// the email as confirmed, deletes the token from cache on success, and selects the appropriate HTML template for
    /// the result.</remarks>
    /// <param name="token">Verification token containing the user identifier and token value used to confirm the email.</param>
    /// <param name="language">Language or culture code used to localize the returned status template.</param>
    /// <returns>An IHtmlRenderable containing the localized email confirmation status view.</returns>
    public Task<IHtmlRenderable> ConfirmEmailAsync(VerificationToken token, string language);
}