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
}
