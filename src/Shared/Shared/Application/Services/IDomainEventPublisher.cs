using _116.Shared.Domain;

namespace _116.Shared.Application.Services;

/// <summary>
/// Interface for publishing domain events.
/// </summary>
public interface IDomainEventPublisher
{
    /// <summary>
    /// Publishes a domain event to all registered handlers.
    /// Handler failures are logged and swallowed so a reaction can never fail
    /// the operation that raised the event.
    /// </summary>
    /// <param name="domainEvent">The domain event to publish.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task Publish(IDomainEvent domainEvent, CancellationToken cancellationToken = default);
}
