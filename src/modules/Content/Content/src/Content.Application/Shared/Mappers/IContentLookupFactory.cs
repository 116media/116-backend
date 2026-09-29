using _116.Content.Domain.Entities;

namespace _116.Content.Application.Shared.Mappers;

/// <summary>
/// Resolves the rows an article, video or lyrics projection names — categories, customers,
/// promotion levels and tags — in one batch per kind, so no projection reads a navigation.
/// </summary>
public interface IContentLookupFactory
{
    /// <summary>
    /// Resolves the lookups a set of articles names.
    /// </summary>
    /// <param name="articles">The articles to project.</param>
    /// <param name="ct">Token to observe for cancellation requests.</param>
    /// <returns>The resolved lookups.</returns>
    Task<ContentLookups> ResolveForArticlesAsync(IReadOnlyList<ArticleEntity> articles, CancellationToken ct = default);

    /// <summary>
    /// Resolves the lookups a set of videos names.
    /// </summary>
    /// <param name="videos">The videos to project.</param>
    /// <param name="ct">Token to observe for cancellation requests.</param>
    /// <returns>The resolved lookups.</returns>
    Task<ContentLookups> ResolveForVideosAsync(IReadOnlyList<VideoEntity> videos, CancellationToken ct = default);

    /// <summary>
    /// Resolves the lookups a set of lyrics pages names.
    /// </summary>
    /// <param name="lyrics">The lyrics pages to project.</param>
    /// <param name="ct">Token to observe for cancellation requests.</param>
    /// <returns>The resolved lookups.</returns>
    Task<ContentLookups> ResolveForLyricsAsync(IReadOnlyList<LyricsEntity> lyrics, CancellationToken ct = default);
}
