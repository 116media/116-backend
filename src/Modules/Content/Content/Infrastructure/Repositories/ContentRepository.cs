using _116.Content.Application.Shared.Repositories;
using _116.Content.Infrastructure.Persistence;
using _116.Shared.Domain;
using _116.Shared.Infrastructure.Repositories;

namespace _116.Content.Infrastructure.Repositories;

/// <summary>
/// Entity Framework implementation of <see cref="IContentRepository{TEntity}" />, registered
/// open-generic behind the interface.
/// </summary>
/// <typeparam name="TEntity">The aggregate type.</typeparam>
/// <param name="context">The Content module database context.</param>
public class ContentRepository<TEntity>(ContentDbContext context)
    : RepositoryBase<ContentDbContext, TEntity, Guid>(context),
        IContentRepository<TEntity>
    where TEntity : class, IEntity<Guid>;
