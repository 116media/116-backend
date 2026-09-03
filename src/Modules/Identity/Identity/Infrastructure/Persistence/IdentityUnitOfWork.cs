using _116.Identity.Application.Shared.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace _116.Identity.Infrastructure.Persistence;

/// <summary>
/// Unit of Work implementation for the Identity module.
/// Coordinates saving changes across all repositories that share the IdentityDbContext.
/// </summary>
/// <param name="context">The Identity module database context.</param>
public class IdentityUnitOfWork(IdentityDbContext context) : IIdentityUnitOfWork
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
