using _116.BuildingBlocks.Infrastructure.Repositories;
using _116.Shared.Domain;
using _116.Storage.Application.Shared.Repositories;
using _116.Storage.Infrastructure.Persistence;

namespace _116.Storage.Infrastructure.Repositories;

/// <summary>
/// Entity Framework implementation of <see cref="IStorageRepository{TEntity}" />, registered
/// open-generic behind the interface.
/// </summary>
/// <typeparam name="TEntity">The aggregate type.</typeparam>
/// <param name="context">The Storage module database context.</param>
public class StorageRepository<TEntity>(StorageDbContext context)
    : RepositoryBase<StorageDbContext, TEntity, Guid>(context),
        IStorageRepository<TEntity>
    where TEntity : class, IEntity<Guid>, IAggregateRoot;
