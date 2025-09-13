namespace _116.Shared.Contracts.Application.CQRS;

/// <summary>
/// Service that dispatches commands and queries to their corresponding handlers.
/// Resolves handlers from the service provider and executes them asynchronously.
/// </summary>
public interface IDispatcher
{
    /// <summary>
    /// Sends a command and returns the response asynchronously.
    /// </summary>
    /// <typeparam name="TResponse">The type of response expected.</typeparam>
    /// <param name="request">The command or query to send.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The response from the handler.</returns>
    Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends a command that doesn't return a response.
    /// </summary>
    /// <param name="request">The command to send.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task Send(IRequest request, CancellationToken cancellationToken = default);
}
