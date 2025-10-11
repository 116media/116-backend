using _116.Shared.Application.Persistence;

namespace _116.Core.Application.Shared.Persistence;

/// <summary>
/// Unit of Work interface specific to the Core module.
/// Coordinates saving changes across all repositories that share the CoreDbContext.
/// </summary>
public interface ICoreUnitOfWork : IUnitOfWork
{
}
