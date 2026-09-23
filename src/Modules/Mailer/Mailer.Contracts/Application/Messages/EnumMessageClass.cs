namespace _116.Mailer.Contracts.Application.Messages;

/// <summary>
/// The delivery policy a message is sent under. The class, not the caller, decides whether
/// recipient preferences may suppress it.
/// </summary>
public enum EnumMessageClass
{
    /// <summary>
    /// Security and account facts. Always sent; preferences are never consulted.
    /// </summary>
    Transactional,

    /// <summary>
    /// Staff work-queue items. In-app always; secondary channels tunable.
    /// </summary>
    Operational,

    /// <summary>
    /// Courtesy updates to users. Preference-gated per channel.
    /// </summary>
    Notification,

    /// <summary>
    /// Opt-in content. Requires a confirmed subscription and is always unsubscribable.
    /// </summary>
    Subscription,
}
