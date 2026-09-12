using System.Data.Common;
using _116.Shared.Application.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace _116.Shared.Infrastructure.Persistence;

/// <summary>
/// Commits one module's context and runs transactions that also cover the other module contexts
/// sharing this scope's connection.
/// </summary>
/// <typeparam name="TContext">The module database context.</typeparam>
/// <param name="context">The module database context.</param>
/// <param name="participants">The other module contexts a transaction started here must cover.</param>
public abstract class UnitOfWorkBase<TContext>(TContext context, IEnumerable<DbContext> participants) : IUnitOfWork
    where TContext : DbContext
{
    /// <inheritdoc />
    public async Task<int> CommitAsync(CancellationToken cancellationToken = default)
    {
        return await context.SaveChangesAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task ExecuteInTransactionAsync(
        Func<CancellationToken, Task> operation,
        CancellationToken cancellationToken = default
    )
    {
        await ExecuteInTransactionAsync<object?>(
            async ct =>
            {
                await operation(ct);

                return null;
            },
            cancellationToken
        );
    }

    /// <inheritdoc />
    public Task<TResult> ExecuteInTransactionAsync<TResult>(
        Func<CancellationToken, Task<TResult>> operation,
        CancellationToken cancellationToken = default
    )
    {
        IExecutionStrategy strategy = context.Database.CreateExecutionStrategy();

        return strategy.ExecuteAsync(
            async ct =>
            {
                await using IDbContextTransaction transaction = await context.Database.BeginTransactionAsync(ct);

                List<DbContext> enlisted = [.. participants.Where(participant => participant != context)];

                if (enlisted.Count > 0)
                {
                    DbTransaction underlying = transaction.GetDbTransaction();

                    foreach (DbContext participant in enlisted)
                    {
                        await participant.Database.UseTransactionAsync(underlying, ct);
                    }
                }

                try
                {
                    TResult result = await operation(ct);

                    await context.SaveChangesAsync(ct);

                    foreach (DbContext participant in enlisted)
                    {
                        await participant.SaveChangesAsync(ct);
                    }

                    await transaction.CommitAsync(ct);

                    return result;
                }
                finally
                {
                    foreach (DbContext participant in enlisted)
                    {
                        await participant.Database.UseTransactionAsync(null, ct);
                    }
                }
            },
            cancellationToken
        );
    }
}
