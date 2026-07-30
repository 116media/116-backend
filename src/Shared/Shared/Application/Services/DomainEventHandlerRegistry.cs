using System.Collections.Concurrent;
using Microsoft.Extensions.DependencyInjection;

namespace _116.Shared.Application.Services;

/// <summary>
/// Reads domain event handler registrations from the service collection the application was
/// composed from. Lookups are cached per handler service type; keyed registrations are ignored
/// because handlers are never registered against a key.
/// </summary>
/// <param name="services">The service collection the application container was built from.</param>
public sealed class DomainEventHandlerRegistry(IServiceCollection services) : IDomainEventHandlerRegistry
{
    private readonly ConcurrentDictionary<Type, IReadOnlyList<ServiceDescriptor>> _descriptorCache = new();

    /// <inheritdoc />
    public IReadOnlyList<ServiceDescriptor> GetHandlerDescriptors(Type handlerServiceType)
    {
        return _descriptorCache.GetOrAdd(
            handlerServiceType,
            serviceType => services.Where(descriptor => Matches(descriptor, serviceType)).ToArray()
        );
    }

    /// <summary>
    /// Determines whether a registration provides the requested handler service type.
    /// </summary>
    /// <param name="descriptor">The registration under inspection.</param>
    /// <param name="handlerServiceType">The closed handler service type being resolved.</param>
    /// <returns>True when the registration provides that service type without a key.</returns>
    private static bool Matches(ServiceDescriptor descriptor, Type handlerServiceType)
    {
        return !descriptor.IsKeyedService && descriptor.ServiceType == handlerServiceType;
    }
}
