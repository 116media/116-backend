using _116.Content.Domain.Entities;
using _116.Shared.Domain;

namespace _116.Content.Application.Shared.Repositories;

/// <summary>
/// Repository interface for translation revision data access operations.
/// </summary>
public interface ITranslationRevisionRepository : IRepository<LyricsTranslationRevisionEntity>
{
    /// <summary>
    /// Retrieves a translation revision by its unique identifier.
    /// Returns null if not found.
    /// </summary>
    /// <param name="id">The unique identifier of the revision.</param>
    /// <param name="cancellationToken">Token to observe for cancellation requests.</param>
    /// <returns>The revision entity if found, otherwise null.</returns>
    Task<LyricsTranslationRevisionEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a translation revision by its unique identifier.
    /// Throws a NotFoundException if not found.
    /// </summary>
    /// <param name="id">The unique identifier of the revision.</param>
    /// <param name="cancellationToken">Token to observe for cancellation requests.</param>
    /// <returns>The revision entity.</returns>
    /// <exception cref="_116.Shared.Application.Exceptions.NotFoundException">Thrown when the revision is not found.</exception>
    Task<LyricsTranslationRevisionEntity> GetByIdOrThrowAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves every revision — pending, accepted, or rejected — proposed against a given
    /// translation, used to render its full review history.
    /// </summary>
    /// <param name="translationId">The translation whose revisions are being listed.</param>
    /// <param name="cancellationToken">Token to observe for cancellation requests.</param>
    /// <returns>The revision history, newest first.</returns>
    Task<IReadOnlyList<LyricsTranslationRevisionEntity>> GetAllByTranslationIdAsync(
        Guid translationId,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Reconciliation query (spec 10's documented ACID safety net): finds every revision that
    /// was marked <c>Accepted</c> but whose <c>ProposedText</c> was never applied to its
    /// translation's current <c>Text</c>. In the normal auto-accept and admin-override paths
    /// both mutations commit together in one call, so this should always be empty in practice —
    /// it exists as a detectable, repairable state for ops rather than an active risk.
    /// </summary>
    /// <param name="cancellationToken">Token to observe for cancellation requests.</param>
    /// <returns>The set of accepted-but-unapplied revisions.</returns>
    Task<IReadOnlyList<LyricsTranslationRevisionEntity>> GetAcceptedButUnappliedAsync(
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Adds a new translation revision to the repository.
    /// </summary>
    /// <param name="revision">The revision entity to add.</param>
    /// <param name="cancellationToken">Token to observe for cancellation requests.</param>
    Task AddAsync(LyricsTranslationRevisionEntity revision, CancellationToken cancellationToken = default);

    /// <summary>
    /// Marks an existing translation revision as modified.
    /// </summary>
    /// <param name="revision">The revision entity to update.</param>
    void Update(LyricsTranslationRevisionEntity revision);
}
