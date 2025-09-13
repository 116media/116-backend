namespace _116.Shared.Domain;

/// <summary>
/// Represents a domain event that occurred within the business domain.
/// Contains metadata about when and what type of event was created.
/// </summary>
public interface IDomainEvent
{
    /// <summary>
    /// Gets the unique identifier for the domain event.
    /// </summary>
    Guid EventId => Guid.NewGuid();

    /// <summary>
    /// Gets the timestamp when the domain event was created.
    /// </summary>
    public DateTime CreatedAt => DateTime.Now;

    /// <summary>
    /// Gets the fully qualified name of the event type.
    /// </summary>
    public string EventType => GetType().AssemblyQualifiedName!;
}
