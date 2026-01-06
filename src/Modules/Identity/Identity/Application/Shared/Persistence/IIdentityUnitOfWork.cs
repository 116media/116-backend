using _116.Shared.Application.Persistence;

namespace _116.Identity.Application.Shared.Persistence;

/// <summary>
/// Unit of Work interface specific to the Identity module.
/// Coordinates saving changes across all repositories that share the IdentityDbContext.
/// </summary>
public interface IIdentityUnitOfWork : IUnitOfWork { }
