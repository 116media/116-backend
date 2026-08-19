using Microsoft.Extensions.Localization;

namespace _116.Mailer.Application.Shared.Errors.Messages;

/// <summary>
/// Provides error messages for the <c>Newsletter</c> domain.
/// Covers subscription input validation and opt-in/opt-out link tokens.
/// </summary>
public class NewsletterErrorMessage(IStringLocalizer<NewsletterErrorMessage> localizer)
{
    /// <summary>
    /// Gets an error message for when the email address is missing.
    /// </summary>
    /// <returns>
    /// An error message indicating the email address is required.
    /// </returns>
    public string EmailRequired()
    {
        return localizer["EmailRequired"];
    }

    /// <summary>
    /// Gets an error message for when the email address is not a valid format.
    /// </summary>
    /// <returns>
    /// An error message indicating the email address format is invalid.
    /// </returns>
    public string EmailInvalid()
    {
        return localizer["EmailInvalid"];
    }

    /// <summary>
    /// Gets an error message for when a confirmation or unsubscribe token
    /// matches no subscriber.
    /// </summary>
    /// <returns>
    /// An error message indicating the link is invalid or expired.
    /// </returns>
    public string TokenInvalid()
    {
        return localizer["TokenInvalid"];
    }
}
