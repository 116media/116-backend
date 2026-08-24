using _116.Core.Application.Shared.Repositories;
using _116.Core.Infrastructure.Persistence;
using _116.Shared.Domain;
using _116.Shared.Infrastructure.Repositories;

namespace _116.Core.Infrastructure.Repositories;

/// <summary>
/// Entity Framework implementation of <see cref="ICoreRepository{TEntity}" />, registered
/// open-generic behind the interface.
/// </summary>
/// <typeparam name="TEntity">The aggregate type.</typeparam>
/// <param name="context">The Core module database context.</param>
public class CoreRepository<TEntity>(CoreDbContext context)
    : RepositoryBase<CoreDbContext, TEntity, Guid>(context),
        ICoreRepository<TEntity>
    where TEntity : class, IEntity<Guid>;
