using _116.Shared.Application.Exceptions;

namespace _116.Shared.Domain;

/// <summary>
/// Data access common to every aggregate: identity reads, existence probes, the tracked load
/// mutation requires, and staging for the next commit. Commit itself never lives here — it
/// belongs to the module's unit of work, so one handler can mutate several aggregates and
/// commit once.
/// </summary>
/// <typeparam name="TEntity">The aggregate type.</typeparam>
/// <typeparam name="TId">The aggregate's identifier type.</typeparam>
public interface IRepository<TEntity, in TId>
    where TEntity : class, IEntity<TId>, IAggregateRoot
    where TId : struct
{
    /// <summary>
    /// Reads an aggregate by identity, or null when none exists.
    /// </summary>
    /// <param name="id">The aggregate identifier.</param>
    /// <param name="cancellationToken">Token to observe for cancellation requests.</param>
    /// <returns>The aggregate, or null.</returns>
    Task<TEntity?> GetByIdAsync(TId id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Reports whether an aggregate with the supplied identity exists.
    /// </summary>
    /// <param name="id">The aggregate identifier.</param>
    /// <param name="cancellationToken">Token to observe for cancellation requests.</param>
    /// <returns>True when the aggregate exists.</returns>
    Task<bool> ExistsAsync(TId id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Throws when no aggregate with the supplied identity exists.
    /// </summary>
    /// <param name="id">The aggregate identifier.</param>
    /// <param name="cancellationToken">Token to observe for cancellation requests.</param>
    /// <exception cref="NotFoundException">Thrown when no aggregate has the supplied id.</exception>
    Task ExistsOrThrowAsync(TId id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Reads a tracked aggregate by identity for mutation.
    /// </summary>
    /// <param name="id">The aggregate identifier.</param>
    /// <param name="cancellationToken">Token to observe for cancellation requests.</param>
    /// <returns>The tracked aggregate.</returns>
    /// <exception cref="NotFoundException">Thrown when no aggregate has the supplied id.</exception>
    Task<TEntity> GetByIdOrThrowAsync(TId id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Stages a new aggregate for insertion on the next commit.
    /// </summary>
    /// <param name="entity">The aggregate to add.</param>
    /// <param name="cancellationToken">Token to observe for cancellation requests.</param>
    Task AddAsync(TEntity entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Stages an existing aggregate for update on the next commit.
    /// </summary>
    /// <param name="entity">The aggregate to update.</param>
    void Update(TEntity entity);

    /// <summary>
    /// Stages an aggregate for deletion on the next commit.
    /// </summary>
    /// <param name="entity">The aggregate to remove.</param>
    void Remove(TEntity entity);
}
