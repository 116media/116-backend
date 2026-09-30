using _116.BuildingBlocks.Infrastructure.Persistence;
using _116.Storage.Application.Shared.Persistence;
using Microsoft.EntityFrameworkCore;

namespace _116.Storage.Infrastructure.Persistence;

/// <summary>
/// Unit of Work implementation for the Storage module.
/// Coordinates saving changes across all repositories that share the StorageDbContext.
/// </summary>
/// <param name="context">The storage database context.</param>
/// <param name="participants">The module contexts sharing this scope's connection.</param>
public class StorageUnitOfWork(StorageDbContext context, IEnumerable<DbContext> participants)
    : UnitOfWorkBase<StorageDbContext>(context, participants),
        IStorageUnitOfWork;
