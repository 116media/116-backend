using System.Text.Json;
using _116.Shared.Domain;

namespace _116.Shared.Infrastructure.Outbox;

/// <summary>
/// A domain event captured durably in the same transaction as the state change that raised it,
/// so a dispatch that dies after the commit can be replayed instead of lost.
/// </summary>
public class OutboxEventEntity
{
    /// <summary>
    /// The event's identifier, carried from the domain event itself.
    /// </summary>
    public Guid Id { get; private set; }

    /// <summary>
    /// The assembly-qualified name the payload deserializes back into.
    /// </summary>
    public string EventType { get; private set; } = null!;

    /// <summary>
    /// The serialized event record.
    /// </summary>
    public string Payload { get; private set; } = null!;

    /// <summary>
    /// When the event occurred, in UTC.
    /// </summary>
    public DateTime OccurredOn { get; private set; }

    /// <summary>
    /// When dispatch succeeded, or null while the event still owes its reactions.
    /// </summary>
    public DateTime? DispatchedAt { get; private set; }

    /// <summary>
    /// How many dispatch attempts the row has taken.
    /// </summary>
    public int AttemptCount { get; private set; }

    /// <summary>
    /// The message from the most recent failed attempt.
    /// </summary>
    public string? LastError { get; private set; }

    private OutboxEventEntity() { }

    /// <summary>
    /// Captures a domain event for durable dispatch.
    /// </summary>
    /// <param name="domainEvent">The event raised by the aggregate.</param>
    /// <returns>The outbox row to persist alongside the state change.</returns>
    public static OutboxEventEntity Create(IDomainEvent domainEvent)
    {
        return new OutboxEventEntity
        {
            Id = domainEvent.EventId,
            EventType = domainEvent.EventType,
            Payload = JsonSerializer.Serialize(domainEvent, domainEvent.GetType()),
            OccurredOn = domainEvent.OccurredOn,
        };
    }

    /// <summary>
    /// Records a successful dispatch, retiring the row from replay.
    /// </summary>
    public void MarkDispatched()
    {
        DispatchedAt = DateTime.UtcNow;
        LastError = null;
    }

    /// <summary>
    /// Records a failed attempt so the replay job can back off and surface dead rows.
    /// </summary>
    /// <param name="error">The failure message.</param>
    public void MarkFailed(string error)
    {
        AttemptCount++;
        LastError = error;
    }

    /// <summary>
    /// Rebuilds the domain event from its stored payload.
    /// </summary>
    /// <returns>The deserialized event, or null when its type no longer exists.</returns>
    public IDomainEvent? ToDomainEvent()
    {
        Type? type = Type.GetType(EventType);
        return type is null ? null : JsonSerializer.Deserialize(Payload, type) as IDomainEvent;
    }
}
