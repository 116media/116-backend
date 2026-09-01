using _116.Core.Application.Shared.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace _116.Core.Infrastructure.Persistence;

/// <summary>
/// Unit of Work implementation for the Core module.
/// Coordinates saving changes across all repositories that share the CoreDbContext.
/// </summary>
public class CoreUnitOfWork(CoreDbContext context) : ICoreUnitOfWork
{
    /// <inheritdoc />
    public async Task<int> CommitAsync(CancellationToken cancellationToken = default)
    {
        return await context.SaveChangesAsync(cancellationToken);
    }

    /// <inheritdoc />
    public Task ExecuteInTransactionAsync(
        Func<CancellationToken, Task> operation,
        CancellationToken cancellationToken = default
    )
    {
        // The strategy owns the transaction so a transient-fault retry replays the whole
        // operation rather than resuming a transaction the retry already lost.
        IExecutionStrategy strategy = context.Database.CreateExecutionStrategy();

        return strategy.ExecuteAsync(
            async ct =>
            {
                await using IDbContextTransaction transaction = await context.Database.BeginTransactionAsync(ct);

                await operation(ct);
                await context.SaveChangesAsync(ct);
                await transaction.CommitAsync(ct);
            },
            cancellationToken
        );
    }
}
