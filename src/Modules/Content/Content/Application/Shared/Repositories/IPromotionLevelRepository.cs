using _116.Content.Domain.Entities;
using _116.Shared.Application.Exceptions;

namespace _116.Content.Application.Shared.Repositories;

/// <summary>
/// Repository interface for managing promotion-level lookup entities.
/// </summary>
public interface IPromotionLevelRepository
{
    /// <summary>
    /// Stages a new promotion level for insertion on the next commit.
    /// </summary>
    /// <param name="promotionLevel">The promotion level to add.</param>
    /// <param name="cancellationToken">Token to observe for cancellation requests.</param>
    Task AddAsync(PromotionLevelEntity promotionLevel, CancellationToken cancellationToken = default);

    /// <summary>
    /// Loads a tracked promotion level by id for mutation.
    /// </summary>
    /// <param name="id">The promotion level identifier.</param>
    /// <param name="cancellationToken">Token to observe for cancellation requests.</param>
    /// <returns>The tracked promotion level.</returns>
    /// <exception cref="NotFoundException">Thrown when no promotion level has the supplied id.</exception>
    Task<PromotionLevelEntity> GetByIdOrThrowAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Reports whether a promotion level already carries the supplied name.
    /// </summary>
    /// <param name="name">The name to probe.</param>
    /// <param name="cancellationToken">Token to observe for cancellation requests.</param>
    /// <returns>True when the name is taken.</returns>
    Task<bool> ExistsByNameAsync(string name, CancellationToken cancellationToken = default);

    /// <summary>
    /// Reads every promotion level, optionally narrowed by a free-text search term.
    /// </summary>
    /// <param name="search">Free-text term, or null for the full list.</param>
    /// <param name="cancellationToken">Token to observe for cancellation requests.</param>
    /// <returns>The promotion levels ordered by name.</returns>
    Task<IReadOnlyList<PromotionLevelEntity>> GetAllAsync(
        string? search = null,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Reads the promotion levels available for selection on new orders.
    /// </summary>
    /// <param name="cancellationToken">Token to observe for cancellation requests.</param>
    /// <returns>The active promotion levels ordered by name.</returns>
    Task<IReadOnlyList<PromotionLevelEntity>> GetActiveAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Stages an existing promotion level for update on the next commit.
    /// </summary>
    /// <param name="promotionLevel">The promotion level to update.</param>
    void Update(PromotionLevelEntity promotionLevel);

    /// <summary>
    /// Resolves the promotion levels the given ids reference, in one query, keyed by id.
    /// Missing ids are simply absent from the result.
    /// </summary>
    /// <param name="ids">The identifiers to resolve.</param>
    /// <param name="cancellationToken">Token to observe for cancellation requests.</param>
    Task<IReadOnlyDictionary<Guid, PromotionLevelEntity>> GetByIdsAsync(
        IReadOnlyCollection<Guid> ids,
        CancellationToken cancellationToken = default
    );
}
