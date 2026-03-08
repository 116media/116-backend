using _116.Content.Application.Shared.Persistence;

namespace _116.Content.Infrastructure.Persistence;

/// <summary>
/// Unit of Work implementation for the Content module.
/// Delegates commit operations to the underlying <see cref="ContentDbContext" />.
/// </summary>
/// <param name="context">The Content module database context.</param>
public class ContentUnitOfWork(ContentDbContext context) : IContentUnitOfWork
{
    /// <inheritdoc />
    public async Task<int> CommitAsync(CancellationToken cancellationToken = default)
    {
        return await context.SaveChangesAsync(cancellationToken);
    }
}
