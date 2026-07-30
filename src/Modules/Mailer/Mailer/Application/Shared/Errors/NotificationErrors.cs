using _116.Mailer.Application.Shared.Errors.Messages;
using _116.Shared.Application.Exceptions;

namespace _116.Mailer.Application.Shared.Errors;

/// <summary>
/// Notification domain error factory providing simple, readable exception creation.
/// Usage: NotificationErrors.NotificationNotFound()
/// </summary>
public class NotificationErrors(NotificationErrorMessage i18n)
{
    /// <summary>
    /// Exposes the localized message provider for use in validators.
    /// </summary>
    public NotificationErrorMessage Msg => i18n;

    /// <summary>
    /// Throws when a notification does not exist for the requesting user —
    /// an unknown id or a row owned by someone else, deliberately
    /// indistinguishable so the response never leaks row existence.
    /// </summary>
    public NotFoundException NotificationNotFound()
    {
        return new NotFoundException(i18n.NotificationNotFound());
    }
}
