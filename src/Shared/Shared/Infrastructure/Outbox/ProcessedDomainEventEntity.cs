namespace _116.Shared.Infrastructure.Outbox;

/// <summary>
/// Records that one handler completed for one event, so a replayed event cannot apply a
/// non-idempotent reaction twice.
/// </summary>
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

    /// <summary>
    /// Records a completed handler invocation.
    /// </summary>
    /// <param name="eventId">The handled event.</param>
    /// <param name="handlerName">The handler that completed.</param>
    /// <returns>The row to persist.</returns>
    public static ProcessedDomainEventEntity Create(Guid eventId, string handlerName)
    {
        return new ProcessedDomainEventEntity
        {
            EventId = eventId,
            HandlerName = handlerName,
            ProcessedAt = DateTime.UtcNow,
        };
    }
}
