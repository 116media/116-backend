using _116.Shared.Application.Persistence;

namespace _116.Core.Infrastructure.Persistence;

/// <summary>
/// Unit of Work implementation for the Core module.
/// Coordinates saving changes across all repositories that share the CoreDbContext.
/// </summary>
public class CoreUnitOfWork(CoreDbContext context) : IUnitOfWork
{
    /// <inheritdoc />
    public async Task<int> CommitAsync(CancellationToken cancellationToken = default)
    {
        return await context.SaveChangesAsync(cancellationToken);
    }
}
