using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Reflection;
using _116.Shared.Domain;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace _116.Shared.Application.Services;

/// <summary>
/// Publishes domain events to all registered event handlers.
/// Resolves handlers from the current scope and invokes them sequentially in
/// registration order through cached compiled delegates. Handler failures are
/// logged and swallowed, and dispatch depth is capped so reentrant
/// publications from handlers that commit their own changes cannot cycle.
/// A handler that cannot be constructed is isolated from its siblings: the
/// fan-out falls back to per-handler construction and the surviving handlers
/// still run.
/// </summary>
/// <param name="serviceProvider">The scope resolving the handlers for one event.</param>
/// <param name="logger">Logger recording resolution and handler failures.</param>
/// <param name="handlerRegistry">Registry of the handler registrations backing each event.</param>
public class DomainEventPublisher(
    IServiceProvider serviceProvider,
    ILogger<DomainEventPublisher> logger,
    IDomainEventHandlerRegistry handlerRegistry
) : IDomainEventPublisher
{
    /// <summary>
    /// Maximum number of nested dispatch levels allowed when handlers raise
    /// further events from their own commits. Publications beyond this depth
    /// are logged and dropped.
    /// </summary>
    public const int MaxDispatchDepth = 3;

    private static readonly AsyncLocal<int> DispatchDepth = new();

    private static readonly ConcurrentDictionary<Type, EventDispatchPlan> DispatchPlanCache = new();

    /// <inheritdoc />
    public async Task Publish(IDomainEvent domainEvent, CancellationToken cancellationToken = default)
    {
        Type eventType = domainEvent.GetType();

        if (DispatchDepth.Value >= MaxDispatchDepth)
        {
            logger.LogWarning(
                "Domain event {EventType} dropped: dispatch depth cap of {MaxDispatchDepth} reached. Event: {DomainEvent}",
                eventType.Name,
                MaxDispatchDepth,
                domainEvent
            );
            return;
        }

        EventDispatchPlan dispatchPlan = DispatchPlanCache.GetOrAdd(eventType, BuildDispatchPlan);

        IReadOnlyList<object> handlers = ResolveHandlers(
            handlerServiceType: dispatchPlan.HandlerServiceType,
            eventType: eventType,
            domainEvent: domainEvent
        );

        if (handlers.Count == 0)
        {
            logger.LogDebug(
                "Domain event {EventType} resolved no handlers. Event: {DomainEvent}",
                eventType.Name,
                domainEvent
            );
            return;
        }

        DispatchDepth.Value++;
        try
        {
            foreach (object handler in handlers)
            {
                try
                {
                    await dispatchPlan.InvokeHandler(handler, domainEvent, cancellationToken);
                }
                catch (Exception exception)
                {
                    logger.LogError(
                        exception,
                        "Domain event handler {HandlerType} failed for event {EventType}. Event: {DomainEvent}",
                        handler.GetType().Name,
                        eventType.Name,
                        domainEvent
                    );
                }
            }
        }
        finally
        {
            DispatchDepth.Value--;
        }
    }

    /// <summary>
    /// Resolves the handlers registered for an event. The whole fan-out is resolved in one call
    /// on the normal path so handler instances stay container-managed; when that resolution
    /// throws, the handlers are constructed one by one so a single unconstructable handler is
    /// isolated and its siblings still run.
    /// </summary>
    /// <param name="handlerServiceType">The closed handler service type to resolve.</param>
    /// <param name="eventType">The concrete domain event type, for logging.</param>
    /// <param name="domainEvent">The event being published, for logging.</param>
    /// <returns>The handlers that could be resolved, in registration order.</returns>
    private IReadOnlyList<object> ResolveHandlers(Type handlerServiceType, Type eventType, IDomainEvent domainEvent)
    {
        try
        {
            return serviceProvider.GetServices(handlerServiceType).OfType<object>().ToArray();
        }
        catch (Exception exception)
        {
            logger.LogError(
                exception,
                "Resolving the handler fan-out for domain event {EventType} failed; falling back to per-handler resolution. Event: {DomainEvent}",
                eventType.Name,
                domainEvent
            );

            return ResolveHandlersIndividually(
                handlerServiceType: handlerServiceType,
                eventType: eventType,
                domainEvent: domainEvent
            );
        }
    }

    /// <summary>
    /// Constructs each registered handler on its own, logging the ones that fail so a broken
    /// registration is diagnosable by handler type instead of silently removing the event's
    /// entire fan-out.
    /// </summary>
    /// <param name="handlerServiceType">The closed handler service type to resolve.</param>
    /// <param name="eventType">The concrete domain event type, for logging.</param>
    /// <param name="domainEvent">The event being published, for logging.</param>
    /// <returns>The handlers that could be constructed, in registration order.</returns>
    private IReadOnlyList<object> ResolveHandlersIndividually(
        Type handlerServiceType,
        Type eventType,
        IDomainEvent domainEvent
    )
    {
        var handlers = new List<object>();

        foreach (ServiceDescriptor descriptor in handlerRegistry.GetHandlerDescriptors(handlerServiceType))
        {
            try
            {
                object? handler = CreateHandler(descriptor);
                if (handler is not null)
                {
                    handlers.Add(handler);
                }
            }
            catch (Exception exception)
            {
                logger.LogError(
                    exception,
                    "Resolving domain event handler {HandlerType} for event {EventType} failed. Event: {DomainEvent}",
                    descriptor.ImplementationType?.Name ?? handlerServiceType.Name,
                    eventType.Name,
                    domainEvent
                );
            }
        }

        return handlers;
    }

    /// <summary>
    /// Produces the instance a single registration stands for, honoring the registered instance,
    /// factory, or implementation type.
    /// </summary>
    /// <param name="descriptor">The registration to materialize.</param>
    /// <returns>The handler instance, or null when the registration yields none.</returns>
    private object? CreateHandler(ServiceDescriptor descriptor)
    {
        if (descriptor.ImplementationInstance is not null)
        {
            return descriptor.ImplementationInstance;
        }

        if (descriptor.ImplementationFactory is not null)
        {
            return descriptor.ImplementationFactory(serviceProvider);
        }

        return descriptor.ImplementationType is null
            ? null
            : ActivatorUtilities.CreateInstance(serviceProvider, descriptor.ImplementationType);
    }

    /// <summary>
    /// Builds the dispatch plan for an event type: the closed handler service
    /// type used to resolve handlers from the container, and a compiled
    /// delegate that invokes a handler without per-call reflection.
    /// </summary>
    /// <param name="eventType">The concrete domain event type.</param>
    /// <returns>The dispatch plan for the event type.</returns>
    private static EventDispatchPlan BuildDispatchPlan(Type eventType)
    {
        Type handlerServiceType = typeof(IDomainEventHandler<>).MakeGenericType(eventType);
        MethodInfo handleMethod = handlerServiceType.GetMethod(nameof(IDomainEventHandler<IDomainEvent>.Handle))!;

        ParameterExpression handlerParameter = Expression.Parameter(typeof(object), "handler");
        ParameterExpression eventParameter = Expression.Parameter(typeof(IDomainEvent), "domainEvent");
        ParameterExpression tokenParameter = Expression.Parameter(typeof(CancellationToken), "cancellationToken");

        MethodCallExpression handleCall = Expression.Call(
            Expression.Convert(handlerParameter, handlerServiceType),
            handleMethod,
            Expression.Convert(eventParameter, eventType),
            tokenParameter
        );

        Func<object, IDomainEvent, CancellationToken, Task> invokeHandler = Expression
            .Lambda<Func<object, IDomainEvent, CancellationToken, Task>>(
                handleCall,
                handlerParameter,
                eventParameter,
                tokenParameter
            )
            .Compile();

        return new EventDispatchPlan(handlerServiceType, invokeHandler);
    }

    /// <summary>
    /// Cached dispatch metadata for a single event type.
    /// </summary>
    /// <param name="HandlerServiceType">The closed <see cref="IDomainEventHandler{TDomainEvent}"/> service type.</param>
    /// <param name="InvokeHandler">The compiled delegate invoking one handler for the event.</param>
    private sealed record EventDispatchPlan(
        Type HandlerServiceType,
        Func<object, IDomainEvent, CancellationToken, Task> InvokeHandler
    );
}
