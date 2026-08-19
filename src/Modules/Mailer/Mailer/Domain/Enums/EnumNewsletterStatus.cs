namespace _116.Mailer.Domain.Enums;

/// <summary>
/// Lifecycle of a newsletter subscriber.
/// </summary>
public enum EnumNewsletterStatus
{
    /// <summary>
    /// Signed up but not yet confirmed; only ever receives the confirmation
    /// email, never newsletter content.
    /// </summary>
    PendingConfirmation,

    /// <summary>
    /// Confirmed through the double opt-in link; eligible for newsletter sends.
    /// </summary>
    Subscribed,

    /// <summary>
    /// Opted out; receives nothing until they re-subscribe.
    /// </summary>
    Unsubscribed,
}
