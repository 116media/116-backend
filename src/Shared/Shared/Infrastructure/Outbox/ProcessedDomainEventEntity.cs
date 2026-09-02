namespace _116.Shared.Infrastructure.Outbox;

/// <summary>
/// Records that one handler completed for one event, so a replayed event cannot apply a
/// non-idempotent reaction twice.
/// </summary>
/// <remarks>
/// Rows are written by an atomic insert that ignores conflicts, never through the change
/// tracker, so this type exists to shape the table rather than to be constructed.
/// </remarks>
public class ProcessedDomainEventEntity
{
    /// <summary>
    /// The event that was handled.
    /// </summary>
    public Guid EventId { get; private set; }

    /// <summary>
    /// The handler that completed.
    /// </summary>
    public string HandlerName { get; private set; } = null!;

    /// <summary>
    /// When the handler completed, in UTC.
    /// </summary>
    public DateTime ProcessedAt { get; private set; }

    private ProcessedDomainEventEntity() { }
}
