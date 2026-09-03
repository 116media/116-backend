using _116.Mailer.Application.Shared.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace _116.Mailer.Infrastructure.Persistence;

/// <summary>
/// Unit of Work implementation for the Mailer module.
/// Delegates commit operations to the underlying <see cref="MailerDbContext" />.
/// </summary>
/// <param name="context">The Mailer module database context.</param>
public class MailerUnitOfWork(MailerDbContext context) : IMailerUnitOfWork
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
