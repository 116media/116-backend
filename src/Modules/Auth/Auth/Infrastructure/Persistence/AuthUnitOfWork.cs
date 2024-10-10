using _116.Shared.Application.Persistence;

namespace _116.Auth.Infrastructure.Persistence;

/// <summary>
/// Unit of Work implementation for the Auth module.
/// Coordinates saving changes across all repositories that share the AuthDbContext.
/// </summary>
public class AuthUnitOfWork(AuthDbContext context) : IUnitOfWork
{
    /// <inheritdoc />
    public async Task<int> CommitAsync(CancellationToken cancellationToken = default)
    {
        return await context.SaveChangesAsync(cancellationToken);
    }
}
