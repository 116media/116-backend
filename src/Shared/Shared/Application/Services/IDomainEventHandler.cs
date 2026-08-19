using _116.Shared.Domain;

namespace _116.Shared.Application.Services;

/// <summary>
/// Interface for handling domain events.
/// Implementations run after the commit that raised the event, in a fresh
/// dependency injection scope, and must re-resolve any entity they need by id.
/// </summary>
/// <typeparam name="TDomainEvent">The type of domain event to handle.</typeparam>
public interface IDomainEventHandler<in TDomainEvent>
    where TDomainEvent : IDomainEvent
{
    /// <summary>
    /// Handles the domain event.
    /// </summary>
    /// <param name="domainEvent">The domain event to handle.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task Handle(TDomainEvent domainEvent, CancellationToken cancellationToken = default);
}
