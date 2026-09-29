namespace _116.Shared.Domain;

/// <summary>
/// A base generic interface for aggregates with a typed identifier.
/// </summary>
/// <typeparam name="T">The type of the aggregate's identifier.</typeparam>
public interface IAggregate<T> : IAggregate, IEntity<T>;

/// <summary>
/// A non-generic base interface for aggregates supporting domain events.
/// </summary>
public interface IAggregate : IEntity
{
    /// <summary>
    /// Gets the list of domain events that have occurred.
    /// </summary>
    IReadOnlyList<IDomainEvent> DomainEvents { get; }

    /// <summary>
    /// Clears the current domain events and returns them.
    /// </summary>
    /// <returns>An array of domain events that were cleared.</returns>
    IDomainEvent[] ClearDomainEvents();
}
