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
    /// Scans the provided assemblies to automatically register all command handlers and query handlers.
    /// Domain event handlers are excluded from scanning by design: each module registers its
    /// <see cref="IDomainEventHandler{TDomainEvent}"/> implementations explicitly so the module
    /// file remains the single readable registry of every reaction in the module.
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

        // The registry reads the handler registrations from this collection. It is built on first
        // resolution, by which time every module has contributed its handlers, so the registration
        // order of the modules relative to this call does not matter.
        services.AddSingleton<IDomainEventHandlerRegistry>(_ => new DomainEventHandlerRegistry(services));

        // Register all handlers (command handlers, query handlers, etc.)
        services.Scan(scan =>
            scan.FromAssemblies(assemblies)
                .AddClasses(
                    classes =>
                        classes.AssignableToAny(
                            typeof(IRequestHandler<>),
                            typeof(IRequestHandler<,>),
                            typeof(ICommandHandler<>),
                            typeof(ICommandHandler<,>),
                            typeof(IQueryHandler<,>)
                        ),
                    publicOnly: true
                )
                .AsImplementedInterfaces()
                .WithScopedLifetime()
        );

        // Register decorators using Scrutor. Decorate order is inner-to-outer: the account-rate-limit
        // decorator is applied first so it is innermost — the throttle runs after validation, on a
        // well-formed request, immediately before the handler. Validation then logging wrap it.
        services.Decorate(typeof(IRequestHandler<,>), typeof(AccountRateLimitDecorator<,>));
        services.Decorate(typeof(IRequestHandler<,>), typeof(ValidationDecorator<,>));
        services.Decorate(typeof(IRequestHandler<,>), typeof(LoggingDecorator<,>));

        // Register FluentValidation validators
        services.AddValidatorsFromAssemblies(assemblies, includeInternalTypes: true);

        return services;
    }
}
