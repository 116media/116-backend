using Microsoft.Extensions.DependencyInjection;

namespace _116.Shared.Application.Services;

/// <summary>
/// Exposes the container registrations that back a domain event's handler fan-out.
/// The publisher reads them to construct handlers one at a time when resolving the
/// fan-out as a whole fails, so a single handler with unresolvable dependencies
/// cannot silence the reactions registered next to it.
/// </summary>
public interface IDomainEventHandlerRegistry
{
    /// <summary>
    /// Returns the registrations made for a closed handler service type, in registration order.
    /// </summary>
    /// <param name="handlerServiceType">
    /// The closed <see cref="IDomainEventHandler{TDomainEvent}"/> service type to look up.
    /// </param>
    /// <returns>The registrations for that service type; empty when the event has no handler.</returns>
    IReadOnlyList<ServiceDescriptor> GetHandlerDescriptors(Type handlerServiceType);
}
