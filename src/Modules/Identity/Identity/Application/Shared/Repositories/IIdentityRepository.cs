using _116.Shared.Domain;

namespace _116.Identity.Application.Shared.Repositories;

/// <summary>
/// Generic repository contract for Identity aggregates keyed by <see cref="Guid" />. A consumer
/// needing only the common operations injects the closed generic directly instead of a
/// bespoke repository.
/// </summary>
/// <typeparam name="TEntity">The aggregate type.</typeparam>
public interface IIdentityRepository<TEntity> : IRepository<TEntity, Guid>
    where TEntity : class, IEntity<Guid>;
