using _116.Mailer.Domain.Enums;

namespace _116.Mailer.Domain.Entities;

/// <summary>
/// Subscription transitions of <see cref="NewsletterSubscriberEntity" />. Its state lives in <c>Entities/NewsletterSubscriberEntity.cs</c>.
/// </summary>
public partial class NewsletterSubscriberEntity
{
    /// <summary>
    /// Confirms the subscription. A no-op unless the row is pending, so
    /// re-clicking the link and confirming an unsubscribed row change nothing.
    /// </summary>
    /// <param name="now">The current UTC time.</param>
    /// <returns><c>true</c> when the state changed to subscribed.</returns>
    public bool Confirm(DateTime now)
    {
        if (Status != EnumNewsletterStatus.PendingConfirmation)
        {
            return false;
        }

        Status = EnumNewsletterStatus.Subscribed;
        ConfirmedAt = now;
        return true;
    }

    /// <summary>
    /// Opts the subscriber out. Idempotent: unsubscribing an already
    /// unsubscribed row is a no-op.
    /// </summary>
    /// <param name="now">The current UTC time.</param>
    /// <returns><c>true</c> when the state changed to unsubscribed.</returns>
    public bool Unsubscribe(DateTime now)
    {
        if (Status == EnumNewsletterStatus.Unsubscribed)
        {
            return false;
        }

        Status = EnumNewsletterStatus.Unsubscribed;
        UnsubscribedAt = now;
        return true;
    }

    /// <summary>
    /// Returns an unsubscribed (or still-pending) row to the pending state
    /// with a fresh confirmation token, for re-subscription and confirmation
    /// re-sends. A no-op on an already subscribed row.
    /// </summary>
    /// <returns><c>true</c> when a fresh confirmation was issued.</returns>
    public bool ReissueConfirmation()
    {
        if (Status == EnumNewsletterStatus.Subscribed)
        {
            return false;
        }

        Status = EnumNewsletterStatus.PendingConfirmation;
        ConfirmationToken = GenerateToken();
        ConfirmedAt = null;
        UnsubscribedAt = null;
        return true;
    }
}
