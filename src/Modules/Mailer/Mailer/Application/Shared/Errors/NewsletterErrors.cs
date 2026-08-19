using _116.Mailer.Application.Shared.Errors.Messages;
using _116.Shared.Application.Exceptions;

namespace _116.Mailer.Application.Shared.Errors;

/// <summary>
/// Newsletter domain error factory providing simple, readable exception creation.
/// Usage: NewsletterErrors.TokenNotFound()
/// </summary>
public class NewsletterErrors(NewsletterErrorMessage i18n)
{
    /// <summary>
    /// Exposes the localized message provider for use in validators.
    /// </summary>
    public NewsletterErrorMessage Msg => i18n;

    /// <summary>
    /// Throws when a confirmation or unsubscribe token matches no subscriber —
    /// a stale, tampered, or rotated link.
    /// </summary>
    public NotFoundException TokenNotFound()
    {
        return new NotFoundException(i18n.TokenInvalid());
    }
}
