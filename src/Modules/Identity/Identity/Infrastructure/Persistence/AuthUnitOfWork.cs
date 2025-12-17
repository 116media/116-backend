using _116.Identity.Application.Shared.Persistence;

namespace _116.Identity.Infrastructure.Persistence;

/// <summary>
/// Unit of Work implementation for the Auth module.
/// Coordinates saving changes across all repositories that share the AuthDbContext.
/// </summary>
public class AuthUnitOfWork(AuthDbContext context) : IAuthUnitOfWork
{
    /// <inheritdoc />
    public async Task<int> CommitAsync(CancellationToken cancellationToken = default)
    {
        return await context.SaveChangesAsync(cancellationToken);
    }
}
