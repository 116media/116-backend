namespace _116.Shared.Application.Persistence;

/// <summary>
/// Unit of Work's pattern for managing database transactions.
/// Use this in handlers to commit changes from multiple repositories atomically.
/// </summary>
public interface IUnitOfWork
{
    /// <summary>
    /// Commits all pending changes to the database in a single transaction.
    /// </summary>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>The number of state entries written to the database.</returns>
    Task<int> CommitAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Runs the operation inside one database transaction, composed with the retrying execution
    /// strategy, committing once at the end. A failure anywhere rolls the whole operation back
    /// instead of stranding half-applied state.
    /// </summary>
    /// <param name="operation">The work to perform before the single commit.</param>
    /// <param name="cancellationToken">Token to observe for cancellation requests.</param>
    Task ExecuteInTransactionAsync(
        Func<CancellationToken, Task> operation,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Runs the operation inside one database transaction and returns its result, on the same
    /// terms as the non-returning overload.
    /// </summary>
    /// <typeparam name="TResult">What the operation produces.</typeparam>
    /// <param name="operation">The work to perform before the single commit.</param>
    /// <param name="cancellationToken">Token to observe for cancellation requests.</param>
    /// <returns>Whatever the operation returned, once the commit succeeded.</returns>
    Task<TResult> ExecuteInTransactionAsync<TResult>(
        Func<CancellationToken, Task<TResult>> operation,
        CancellationToken cancellationToken = default
    );
}
