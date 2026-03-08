using _116.Shared.Application.Persistence;

namespace _116.Content.Application.Shared.Persistence;

/// <summary>
/// Unit of Work interface specific to the Content module.
/// Coordinates saving changes across all repositories that share the ContentDbContext.
/// </summary>
public interface IContentUnitOfWork : IUnitOfWork { }
