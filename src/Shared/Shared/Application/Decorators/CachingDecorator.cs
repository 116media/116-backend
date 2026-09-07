using _116.Shared.Contracts.Application.CQRS;
using Microsoft.Extensions.Caching.Hybrid;

namespace _116.Shared.Application.Decorators;

/// <summary>
/// Decorator that serves <see cref="ICacheableRequest" /> results from the hybrid cache and
/// stores every miss. Requests that do not opt in pass straight through.
/// </summary>
/// <typeparam name="TRequest">The type of the request.</typeparam>
/// <typeparam name="TResponse">The type of the response.</typeparam>
/// <param name="handler">The decorated handler.</param>
/// <param name="cache">The hybrid cache backing every cached result.</param>
public class CachingDecorator<TRequest, TResponse>(IRequestHandler<TRequest, TResponse> handler, HybridCache cache)
    : IRequestHandler<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
    where TResponse : notnull
{
    /// <summary>
    /// Handles the request, returning the stored result when one exists for its key.
    /// </summary>
    public async Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken = default)
    {
        if (
            request is not ICacheableRequest cacheable
            || request is IConditionallyCacheableRequest { IsCacheable: false }
        )
        {
            return await handler.Handle(request, cancellationToken);
        }

        var options = new HybridCacheEntryOptions { Expiration = cacheable.Ttl, LocalCacheExpiration = cacheable.Ttl };

        // The state overload keeps the factory static, so no closure is allocated per request.
        return await cache.GetOrCreateAsync(
            options: options,
            key: cacheable.CacheKey,
            state: (handler, request),
            tags: cacheable.CacheTags,
            cancellationToken: cancellationToken,
            factory: static async (state, token) => await state.handler.Handle(state.request, token)
        );
    }
}
