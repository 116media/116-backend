namespace _116.Shared.Domain;

/// <summary>
/// A fact the domain records at the moment it happens. Identity and timestamp are stamped once
/// at construction so the event can be stored, deduplicated and replayed.
/// </summary>
public interface IDomainEvent
{
    /// <summary>
    /// The event's stable unique identifier.
    /// </summary>
    Guid EventId { get; }

    /// <summary>
    /// When the event occurred, in UTC.
    /// </summary>
    DateTime OccurredOn { get; }

    /// <summary>
    /// The fully qualified name of the event type.
    /// </summary>
    string EventType { get; }
}

/// <summary>
/// Base record for all domain events; stamps identity and time exactly once.
/// </summary>
/// <remarks>
/// Identity and time are settable only at construction so rebuilding an event from its stored
/// payload restores the stamp it was raised with. A replayed event that minted a fresh id would
/// defeat the processed-event guard, and every non-idempotent handler would run twice.
/// </remarks>
public abstract record DomainEvent : IDomainEvent
{
    /// <inheritdoc />
    public Guid EventId { get; init; } = Guid.NewGuid();

    /// <inheritdoc />
    public DateTime OccurredOn { get; init; } = DateTime.UtcNow;

    /// <inheritdoc />
    public string EventType => GetType().AssemblyQualifiedName!;

    /// <summary>
    /// Compares only the event's type, leaving the derived record's synthesized equality to
    /// compare the payload. The stamped id and time are delivery metadata, so two events
    /// describing the same fact stay equal.
    /// </summary>
    /// <param name="other">The event to compare against.</param>
    /// <returns>True when both events are of the same type.</returns>
    public virtual bool Equals(DomainEvent? other)
    {
        return other is not null && EqualityContract == other.EqualityContract;
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        return EqualityContract.GetHashCode();
    }
}
