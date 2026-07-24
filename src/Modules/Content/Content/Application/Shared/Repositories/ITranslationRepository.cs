using _116.Content.Domain.Entities;
using _116.Shared.Domain;

namespace _116.Content.Application.Shared.Repositories;

/// <summary>
/// Repository interface for lyrics translation data access operations.
/// </summary>
public interface ITranslationRepository : IRepository<LyricsTranslationEntity>
{
    /// <summary>
    /// Retrieves a lyrics translation by its unique identifier.
    /// Returns null if not found.
    /// </summary>
    /// <param name="id">The unique identifier of the translation.</param>
    /// <param name="cancellationToken">Token to observe for cancellation requests.</param>
    /// <returns>The translation entity if found, otherwise null.</returns>
    Task<LyricsTranslationEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a lyrics translation by its unique identifier.
    /// Throws a NotFoundException if not found.
    /// </summary>
    /// <param name="id">The unique identifier of the translation.</param>
    /// <param name="cancellationToken">Token to observe for cancellation requests.</param>
    /// <returns>The translation entity.</returns>
    /// <exception cref="_116.Shared.Application.Exceptions.NotFoundException">Thrown when the translation is not found.</exception>
    Task<LyricsTranslationEntity> GetByIdOrThrowAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves the translation for a given lyrics page and language pair. Returns null if no
    /// translation has been requested yet for that pair — the caller falls back to calling
    /// <c>ITranslationService</c> in that case (the idempotent AI-generation entry point).
    /// </summary>
    /// <param name="lyricsId">The lyrics page to look up.</param>
    /// <param name="language">ISO 639-1 language code to look up.</param>
    /// <param name="cancellationToken">Token to observe for cancellation requests.</param>
    /// <returns>The translation entity if found, otherwise null.</returns>
    Task<LyricsTranslationEntity?> GetByLyricsAndLanguageAsync(
        Guid lyricsId,
        string language,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Retrieves every translation of a lyrics page, across all requested languages.
    /// </summary>
    /// <param name="lyricsId">The lyrics page to look up.</param>
    /// <param name="cancellationToken">Token to observe for cancellation requests.</param>
    /// <returns>The list of translations for the lyrics page, empty if none have been requested.</returns>
    Task<IReadOnlyList<LyricsTranslationEntity>> GetAllByLyricsIdAsync(
        Guid lyricsId,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Adds a new lyrics translation to the repository.
    /// </summary>
    /// <param name="translation">The translation entity to add.</param>
    /// <param name="cancellationToken">Token to observe for cancellation requests.</param>
    Task AddAsync(LyricsTranslationEntity translation, CancellationToken cancellationToken = default);

    /// <summary>
    /// Marks an existing lyrics translation as modified.
    /// </summary>
    /// <param name="translation">The translation entity to update.</param>
    void Update(LyricsTranslationEntity translation);
}
