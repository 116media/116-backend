using _116.BuildingBlocks.Application.Persistence;

namespace _116.Storage.Application.Shared.Persistence;

/// <summary>
/// Unit of Work interface specific to the Storage module.
/// Coordinates saving changes across all repositories that share the StorageDbContext.
/// </summary>
public interface IStorageUnitOfWork : IUnitOfWork { }
