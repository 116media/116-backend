using _116.Identity.Application.Shared.Persistence;

namespace _116.Identity.Infrastructure.Persistence;

/// <summary>
/// Unit of Work implementation for the Identity module.
/// Coordinates saving changes across all repositories that share the IdentityDbContext.
/// </summary>
public class IdentityUnitOfWork(IdentityDbContext context) : IIdentityUnitOfWork
{
    /// <inheritdoc />
    public async Task<int> CommitAsync(CancellationToken cancellationToken = default)
    {
        return await context.SaveChangesAsync(cancellationToken);
    }
}
