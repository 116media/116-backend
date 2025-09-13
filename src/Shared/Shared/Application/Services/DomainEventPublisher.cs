using System.Reflection;
using _116.Shared.Domain;
using Microsoft.Extensions.DependencyInjection;

namespace _116.Shared.Application.Services;

/// <summary>
/// Interface for publishing domain events.
/// </summary>
public interface IDomainEventPublisher
{
    /// <summary>
    /// Publishes a domain event to all registered handlers.
    /// </summary>
    /// <param name="domainEvent">The domain event to publish.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task Publish(IDomainEvent domainEvent, CancellationToken cancellationToken = default);
}

/// <summary>
/// Interface for handling domain events.
/// </summary>
/// <typeparam name="TDomainEvent">The type of domain event to handle.</typeparam>
public interface IDomainEventHandler<in TDomainEvent> where TDomainEvent : IDomainEvent
{
    /// <summary>
    /// Handles the domain event.
    /// </summary>
    /// <param name="domainEvent">The domain event to handle.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task Handle(TDomainEvent domainEvent, CancellationToken cancellationToken = default);
}

/// <summary>
/// Publishes domain events to all registered event handlers.
/// Resolves handlers dynamically based on the event type and executes them concurrently.
/// </summary>
public class DomainEventPublisher(IServiceProvider serviceProvider) : IDomainEventPublisher
{
    /// <summary>
    /// Publishes a domain event to all registered handlers.
    /// </summary>
    public async Task Publish(IDomainEvent domainEvent, CancellationToken cancellationToken = default)
    {
        Type eventType = domainEvent.GetType();
        Type handlerType = typeof(IDomainEventHandler<>).MakeGenericType(eventType);

        IEnumerable<object?> handlers = serviceProvider.GetServices(handlerType);

        IEnumerable<Task> tasks = handlers.Select(async handler =>
        {
            MethodInfo? handleMethod = handlerType.GetMethod("Handle");
            if (handleMethod != null)
            {
                object? result = handleMethod.Invoke(handler, [domainEvent, cancellationToken]);
                if (result is Task task) await task;
            }
        });

        await Task.WhenAll(tasks);
    }
}
