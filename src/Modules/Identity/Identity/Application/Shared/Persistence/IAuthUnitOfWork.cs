using _116.Shared.Application.Persistence;

namespace _116.Identity.Application.Shared.Persistence;

/// <summary>
/// Unit of Work interface specific to the Auth module.
/// Coordinates saving changes across all repositories that share the IdentityDbContext.
/// </summary>
public interface IAuthUnitOfWork : IUnitOfWork
{
}
