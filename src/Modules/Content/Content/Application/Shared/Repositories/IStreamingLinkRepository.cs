using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Shared.Domain;

namespace _116.Content.Application.Shared.Repositories;

/// <summary>
/// Repository interface for streaming platform link data access operations.
/// </summary>
public interface IStreamingLinkRepository : IRepository<StreamingLinkEntity>
{
    /// <summary>
    /// Retrieves the curated streaming link for a given album and platform. Returns null if
    /// no curated link has been set for that platform — the caller falls back to a generated
    /// search URL in that case.
    /// </summary>
    /// <param name="albumId">The album to look up.</param>
    /// <param name="platform">The streaming platform to look up.</param>
    /// <param name="cancellationToken">Token to observe for cancellation requests.</param>
    /// <returns>The streaming link entity if found, otherwise null.</returns>
    Task<StreamingLinkEntity?> GetByAlbumAndPlatformAsync(
        Guid albumId,
        EnumStreamingPlatform platform,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Retrieves every curated streaming link belonging to an album, keyed by platform.
    /// Platforms with no curated link are absent from the dictionary.
    /// </summary>
    /// <param name="albumId">The album to look up.</param>
    /// <param name="cancellationToken">Token to observe for cancellation requests.</param>
    /// <returns>A read-only dictionary mapping each curated platform to its URL.</returns>
    Task<IReadOnlyDictionary<EnumStreamingPlatform, string>> GetByAlbumAsync(
        Guid albumId,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Retrieves the curated streaming link for a given standalone single and platform.
    /// Returns null if no curated link has been set for that platform — the caller falls back
    /// to a generated search URL in that case.
    /// </summary>
    /// <param name="lyricsId">The standalone single (lyrics page) to look up.</param>
    /// <param name="platform">The streaming platform to look up.</param>
    /// <param name="cancellationToken">Token to observe for cancellation requests.</param>
    /// <returns>The streaming link entity if found, otherwise null.</returns>
    Task<StreamingLinkEntity?> GetByLyricsAndPlatformAsync(
        Guid lyricsId,
        EnumStreamingPlatform platform,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Retrieves every curated streaming link belonging to a standalone single, keyed by platform.
    /// Platforms with no curated link are absent from the dictionary.
    /// </summary>
    /// <param name="lyricsId">The standalone single (lyrics page) to look up.</param>
    /// <param name="cancellationToken">Token to observe for cancellation requests.</param>
    /// <returns>A read-only dictionary mapping each curated platform to its URL.</returns>
    Task<IReadOnlyDictionary<EnumStreamingPlatform, string>> GetByLyricsAsync(
        Guid lyricsId,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Adds a new streaming link to the repository.
    /// </summary>
    Task AddAsync(StreamingLinkEntity streamingLink, CancellationToken cancellationToken = default);

    /// <summary>
    /// Marks an existing streaming link as modified.
    /// </summary>
    void Update(StreamingLinkEntity streamingLink);

    /// <summary>
    /// Marks a streaming link for deletion from the repository.
    /// </summary>
    void Remove(StreamingLinkEntity streamingLink);
}
