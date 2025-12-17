using _116.Shared.Application.Persistence;

namespace _116.Auth.Application.Shared.Persistence;

/// <summary>
/// Unit of Work interface specific to the Auth module.
/// Coordinates saving changes across all repositories that share the AuthDbContext.
/// </summary>
public interface IAuthUnitOfWork : IUnitOfWork
{
}
