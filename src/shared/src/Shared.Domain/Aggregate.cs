namespace _116.Shared.Domain;

/// <summary>
/// An abstract aggregate root with domain event support.
/// </summary>
/// <typeparam name="TId">The type of the aggregate identifier.</typeparam>
public abstract class Aggregate<TId> : Entity<TId>, IAggregate<TId>, IAggregateRoot
{
    private readonly List<IDomainEvent> _domainEvents = [];

    /// <inheritdoc />
    public IReadOnlyList<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    /// <summary>
    /// Records a domain event on this aggregate. Callable only by the aggregate itself.
    /// </summary>
    /// <param name="domainEvent">The domain event to add.</param>
    protected void AddDomainEvent(IDomainEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }

    /// <inheritdoc />
    public IDomainEvent[] ClearDomainEvents()
    {
        IDomainEvent[] dequeuedDomainEvents = _domainEvents.ToArray();
        _domainEvents.Clear();
        return dequeuedDomainEvents;
    }
}
