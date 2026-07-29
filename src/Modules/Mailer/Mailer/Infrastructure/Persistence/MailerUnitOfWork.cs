using _116.Mailer.Application.Shared.Persistence;

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
}
