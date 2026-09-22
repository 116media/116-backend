using _116.Shared.Contracts.Application.CQRS;
using Microsoft.Extensions.DependencyInjection;

namespace _116.Shared.Application.Services;

/// <summary>
/// Untyped dispatch seam for requests returning a response. One concrete wrapper is built per
/// request type and cached for the process lifetime.
/// </summary>
/// <typeparam name="TResponse">The response the request produces.</typeparam>
internal abstract class RequestHandlerWrapper<TResponse>
{
    /// <summary>
    /// Resolves the typed handler and invokes it.
    /// </summary>
    /// <param name="request">The request instance.</param>
    /// <param name="serviceProvider">Scope the handler is resolved from.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The handler response.</returns>
    public abstract Task<TResponse> Handle(
        object request,
        IServiceProvider serviceProvider,
        CancellationToken cancellationToken
    );
}

/// <summary>
/// Untyped dispatch seam for requests returning no response. One concrete wrapper is built per
/// request type and cached for the process lifetime.
/// </summary>
internal abstract class RequestHandlerWrapper
{
    /// <summary>
    /// Resolves the typed handler and invokes it.
    /// </summary>
    /// <param name="request">The request instance.</param>
    /// <param name="serviceProvider">Scope the handler is resolved from.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task representing the handler invocation.</returns>
    public abstract Task Handle(object request, IServiceProvider serviceProvider, CancellationToken cancellationToken);
}

/// <summary>
/// Bridges the untyped dispatch path onto <see cref="IRequestHandler{TRequest, TResponse}" />
/// through a virtual call, so no reflection runs per request.
/// </summary>
/// <typeparam name="TRequest">The concrete request type.</typeparam>
/// <typeparam name="TResponse">The response the request produces.</typeparam>
internal sealed class RequestHandlerWrapperImpl<TRequest, TResponse> : RequestHandlerWrapper<TResponse>
    where TRequest : IRequest<TResponse>
{
    /// <inheritdoc />
    public override Task<TResponse> Handle(
        object request,
        IServiceProvider serviceProvider,
        CancellationToken cancellationToken
    )
    {
        var handler = serviceProvider.GetService<IRequestHandler<TRequest, TResponse>>();

        if (handler is null)
        {
            throw new InvalidOperationException($"No handler registered for {typeof(TRequest).Name}");
        }

        return handler.Handle((TRequest)request, cancellationToken);
    }
}

/// <summary>
/// Bridges the untyped dispatch path onto <see cref="IRequestHandler{TRequest}" /> through a
/// virtual call, so no reflection runs per request.
/// </summary>
/// <typeparam name="TRequest">The concrete request type.</typeparam>
internal sealed class RequestHandlerWrapperImpl<TRequest> : RequestHandlerWrapper
    where TRequest : IRequest
{
    /// <inheritdoc />
    public override Task Handle(object request, IServiceProvider serviceProvider, CancellationToken cancellationToken)
    {
        var handler = serviceProvider.GetService<IRequestHandler<TRequest>>();

        if (handler is null)
        {
            throw new InvalidOperationException($"No handler registered for {typeof(TRequest).Name}");
        }

        return handler.Handle((TRequest)request, cancellationToken);
    }
}
