using _116.Shared.Application.Builders.RateLimit;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Shared.Application.Decorators;

/// <summary>
/// Decorator that throttles a command per target account before the handler runs, when the command
/// opts in via <see cref="IAccountRateLimited" />. Commands that do not implement it pass straight
/// through after a single type check. Registered as the innermost decorator so the throttle runs
/// after validation, on a well-formed request, immediately before the handler.
/// </summary>
/// <typeparam name="TRequest">The type of the request.</typeparam>
/// <typeparam name="TResponse">The type of the response.</typeparam>
public class AccountRateLimitDecorator<TRequest, TResponse>(
    IRequestHandler<TRequest, TResponse> handler,
    IAccountRateLimiter accountRateLimiter
) : IRequestHandler<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    /// <inheritdoc />
    public async Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken = default)
    {
        if (request is IAccountRateLimited limited)
        {
            await accountRateLimiter.EnsureWithinLimitAsync(
                limited.RateLimitPolicy,
                limited.AccountKey,
                cancellationToken
            );
        }

        return await handler.Handle(request, cancellationToken);
    }
}
