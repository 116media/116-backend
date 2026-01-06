using System.Collections.Concurrent;
using System.Reflection;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Shared.Application.Services;

/// <summary>
/// Dispatches command and query requests to their corresponding handlers.
/// Resolves handlers from the service provider and invokes them with the provided request data.
/// Use caching for improved performance when resolving handler types.
/// </summary>
public class Dispatcher(IServiceProvider serviceProvider) : IDispatcher
{
    // Cache for performance optimization
    private static readonly ConcurrentDictionary<Type, Type> HandlerTypeCache = new();

    /// <summary>
    /// Sends a request and returns the response asynchronously.
    /// </summary>
    public async Task<TResponse> Send<TResponse>(
        IRequest<TResponse> request,
        CancellationToken cancellationToken = default
    )
    {
        Type requestType = request.GetType();
        Type handlerType = GetHandlerType(requestType, typeof(TResponse));

        object? handler = serviceProvider.GetService(handlerType);

        if (handler == null)
        {
            throw new InvalidOperationException($"No handler registered for {requestType.Name}");
        }

        MethodInfo? handleMethod = handlerType.GetMethod("Handle", [requestType, typeof(CancellationToken)]);

        if (handleMethod == null)
        {
            throw new InvalidOperationException($"Handler for {requestType.Name} does not implement Handle method");
        }

        object[] parameters = [request, cancellationToken];
        object? result = handleMethod.Invoke(handler, parameters);

        if (result is Task<TResponse> task)
        {
            return await task;
        }

        throw new InvalidOperationException($"Handler method must return Task<{typeof(TResponse).Name}>");
    }

    /// <summary>
    /// Sends a request that doesn't return a response.
    /// </summary>
    public async Task Send(IRequest request, CancellationToken cancellationToken = default)
    {
        Type requestType = request.GetType();
        Type handlerType = GetHandlerType(requestType);

        object? handler = serviceProvider.GetService(handlerType);

        if (handler == null)
        {
            throw new InvalidOperationException($"No handler registered for {requestType.Name}");
        }

        MethodInfo? handleMethod = handlerType.GetMethod("Handle", [requestType, typeof(CancellationToken)]);

        if (handleMethod == null)
        {
            throw new InvalidOperationException($"Handler for {requestType.Name} does not implement Handle method");
        }

        object[] parameters = [request, cancellationToken];
        object? result = handleMethod.Invoke(handler, parameters);

        if (result is Task task)
        {
            await task;
        }
        else
        {
            throw new InvalidOperationException("Handler method must return Task");
        }
    }

    /// <summary>
    /// Gets the handler type for a request with response.
    /// </summary>
    private static Type GetHandlerType(Type requestType, Type responseType)
    {
        return HandlerTypeCache.GetOrAdd(
            requestType,
            static (key, responseType) =>
            {
                Type handlerType = typeof(IRequestHandler<,>).MakeGenericType(key, responseType);
                return handlerType;
            },
            responseType
        );
    }

    /// <summary>
    /// Gets the handler type for a request without response.
    /// </summary>
    private static Type GetHandlerType(Type requestType)
    {
        return HandlerTypeCache.GetOrAdd(
            requestType,
            static key =>
            {
                Type handlerType = typeof(IRequestHandler<>).MakeGenericType(key);
                return handlerType;
            }
        );
    }
}
