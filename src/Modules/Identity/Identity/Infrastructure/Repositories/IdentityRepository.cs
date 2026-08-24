using _116.Identity.Application.Shared.Repositories;
using _116.Identity.Infrastructure.Persistence;
using _116.Shared.Domain;
using _116.Shared.Infrastructure.Repositories;

namespace _116.Identity.Infrastructure.Repositories;

/// <summary>
/// Entity Framework implementation of <see cref="IIdentityRepository{TEntity}" />, registered
/// open-generic behind the interface.
/// </summary>
/// <typeparam name="TEntity">The aggregate type.</typeparam>
/// <param name="context">The Identity module database context.</param>
public class IdentityRepository<TEntity>(IdentityDbContext context)
    : RepositoryBase<IdentityDbContext, TEntity, Guid>(context),
        IIdentityRepository<TEntity>
    where TEntity : class, IEntity<Guid>;
