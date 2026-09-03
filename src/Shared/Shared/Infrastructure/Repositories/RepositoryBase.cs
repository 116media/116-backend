using _116.Shared.Application.Exceptions;
using _116.Shared.Domain;
using _116.Shared.Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;

namespace _116.Shared.Infrastructure.Repositories;

/// <summary>
/// Entity Framework implementation of <see cref="IRepository{TEntity, TId}" />. Derived types
/// override <see cref="Query" /> to declare the aggregate's hydration graph once.
/// </summary>
/// <typeparam name="TContext">The module database context.</typeparam>
/// <typeparam name="TEntity">The aggregate type.</typeparam>
/// <typeparam name="TId">The aggregate's identifier type.</typeparam>
/// <param name="context">The module database context.</param>
public abstract class RepositoryBase<TContext, TEntity, TId>(TContext context) : IRepository<TEntity, TId>
    where TContext : DbContext
    where TEntity : class, IEntity<TId>, IAggregateRoot
    where TId : struct
{
    /// <summary>
    /// The module database context.
    /// </summary>
    protected TContext Context => context;

    /// <summary>
    /// The query root carrying every navigation this aggregate's reads need. Tracking follows
    /// the module default; override to add <c>Include</c> calls rather than repeating them per
    /// finder.
    /// </summary>
    protected virtual IQueryable<TEntity> Query() => context.Set<TEntity>();

    /// <summary>
    /// The tracked query root, carrying the same hydration graph as <see cref="Query" />. Every
    /// read whose result will be mutated goes through this.
    /// </summary>
    protected IQueryable<TEntity> QueryTracked() => Query().AsTracking();

    /// <inheritdoc />
    public virtual async Task<TEntity?> GetByIdAsync(TId id, CancellationToken cancellationToken = default)
    {
        return await Query().FirstOrDefaultAsync(entity => entity.Id.Equals(id), cancellationToken);
    }

    /// <inheritdoc />
    public virtual async Task<TEntity> GetByIdOrThrowAsync(TId id, CancellationToken cancellationToken = default)
    {
        return await QueryTracked()
            .Where(entity => entity.Id.Equals(id))
            .FirstDefaultOrThrowAsync(keyValue: id, cancellationToken: cancellationToken);
    }

    /// <inheritdoc />
    public virtual async Task<bool> ExistsAsync(TId id, CancellationToken cancellationToken = default)
    {
        return await context.Set<TEntity>().AnyAsync(entity => entity.Id.Equals(id), cancellationToken);
    }

    /// <inheritdoc />
    public virtual async Task ExistsOrThrowAsync(TId id, CancellationToken cancellationToken = default)
    {
        bool exists = await ExistsAsync(id: id, cancellationToken: cancellationToken);

        if (!exists)
        {
            throw new NotFoundException(typeof(TEntity).Name, id);
        }
    }

    /// <inheritdoc />
    public virtual async Task AddAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        await context.Set<TEntity>().AddAsync(entity, cancellationToken);
    }

    /// <inheritdoc />
    public virtual void Update(TEntity entity)
    {
        context.Set<TEntity>().Update(entity);
    }

    /// <inheritdoc />
    public virtual void Remove(TEntity entity)
    {
        context.Set<TEntity>().Remove(entity);
    }
}
