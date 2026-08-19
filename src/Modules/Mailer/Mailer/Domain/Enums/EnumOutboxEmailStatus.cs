namespace _116.Mailer.Domain.Enums;

/// <summary>
/// Lifecycle of an outbox email row.
/// </summary>
public enum EnumOutboxEmailStatus
{
    /// <summary>
    /// Waiting for delivery; the dispatcher picks it up when its next attempt
    /// time is due.
    /// </summary>
    Pending,

    /// <summary>
    /// Delivered to the provider successfully.
    /// </summary>
    Sent,

    /// <summary>
    /// Given up: a permanent provider failure, or the retry schedule was
    /// exhausted. Terminal until an operator re-queues the row.
    /// </summary>
    Failed,
}
