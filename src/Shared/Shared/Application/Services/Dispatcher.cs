using System.Collections.Concurrent;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Shared.Application.Services;

/// <summary>
/// Dispatches command and query requests to their corresponding handlers, resolved from the
/// service provider through a cached typed wrapper per request type.
/// </summary>
public class Dispatcher(IServiceProvider serviceProvider) : IDispatcher
{
    private static readonly ConcurrentDictionary<(Type RequestType, Type ResponseType), object> ResponseWrapperCache =
        new();

    private static readonly ConcurrentDictionary<Type, RequestHandlerWrapper> VoidWrapperCache = new();

    /// <summary>
    /// Sends a request and returns the response asynchronously.
    /// </summary>
    public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
    {
        var wrapper =
            (RequestHandlerWrapper<TResponse>)
                ResponseWrapperCache.GetOrAdd(
                    (request.GetType(), typeof(TResponse)),
                    static key =>
                        Activator.CreateInstance(
                            typeof(RequestHandlerWrapperImpl<,>).MakeGenericType(key.RequestType, key.ResponseType)
                        )!
                );

        return wrapper.Handle(request, serviceProvider, cancellationToken);
    }

    /// <summary>
    /// Sends a request that doesn't return a response.
    /// </summary>
    public Task Send(IRequest request, CancellationToken cancellationToken = default)
    {
        RequestHandlerWrapper wrapper = VoidWrapperCache.GetOrAdd(
            request.GetType(),
            static requestType =>
                (RequestHandlerWrapper)
                    Activator.CreateInstance(typeof(RequestHandlerWrapperImpl<>).MakeGenericType(requestType))!
        );

        return wrapper.Handle(request, serviceProvider, cancellationToken);
    }
}
