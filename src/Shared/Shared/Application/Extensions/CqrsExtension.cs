using System.Reflection;
using _116.Shared.Application.Decorators;
using _116.Shared.Application.Services;
using _116.Shared.Contracts.Application.CQRS;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace _116.Shared.Application.Extensions;

/// <summary>
/// Extension methods for registering CQRS services and handlers.
/// </summary>
public static class CqrsExtension
{
    /// <summary>
    /// Registers CQRS services including dispatcher, handlers, domain event publisher and validators.
    /// Scans the provided assemblies to automatically register all command handlers, query handlers, and domain event handlers.
    /// </summary>
    /// <param name="services">The service collection to register services with.</param>
    /// <param name="assemblies">The assemblies to scan for handlers and validators.</param>
    /// <returns>The updated <see cref="IServiceCollection"/> for method chaining.</returns>
    public static IServiceCollection AddCqrsWithAssemblies(
        this IServiceCollection services,
        params Assembly[] assemblies
    )
    {
        // Register the custom dispatcher
        services.AddScoped<IDispatcher, Dispatcher>();

        // Register domain event publisher
        services.AddScoped<IDomainEventPublisher, DomainEventPublisher>();

        // Register all handlers (command handlers, query handlers, etc.)
        services.Scan(scan => scan
            .FromAssemblies(assemblies)
            .AddClasses(classes => classes.AssignableToAny(
                typeof(IRequestHandler<>),
                typeof(IRequestHandler<,>),
                typeof(ICommandHandler<>),
                typeof(ICommandHandler<,>),
                typeof(IQueryHandler<,>)))
            .AsImplementedInterfaces()
            .WithScopedLifetime());

        // TODO: Add decorators later - for now just get the basic functionality working

        // Register domain event handlers
        services.Scan(scan => scan
            .FromAssemblies(assemblies)
            .AddClasses(classes => classes.AssignableToAny(typeof(IDomainEventHandler<>)))
            .AsImplementedInterfaces()
            .WithScopedLifetime());

        // Register FluentValidation validators
        services.AddValidatorsFromAssemblies(assemblies);

        return services;
    }
}
