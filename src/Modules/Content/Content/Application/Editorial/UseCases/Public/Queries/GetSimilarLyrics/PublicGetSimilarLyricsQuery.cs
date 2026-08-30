using _116.Content.Application.Shared.Cache;
using _116.Content.Application.Shared.DTOs;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Editorial.UseCases.Public.Queries.GetSimilarLyrics;

/// <summary>
/// Query for retrieving lyrics pages similar to a given lyrics page (spec 06): a three-way
/// waterfall across shared video category, shared tags, and recency among standalone pages.
/// </summary>
/// <param name="LyricsId">The source lyrics page to find similar pages for.</param>
/// <param name="CurrentUserId">
/// The authenticated caller's id, or null for an anonymous request. When null, the per-user
/// <c>IsLiked</c> flag on the returned summaries resolves to false.
/// </param>
public record PublicGetSimilarLyricsQuery(Guid LyricsId, Guid? CurrentUserId = null)
    : IQuery<PublicGetSimilarLyricsResult>,
        IConditionallyCacheableRequest
{
    /// <inheritdoc />
    /// <remarks>
    /// Only the anonymous projection is stored: an authenticated response carries per-user
    /// interaction flags, and caching it would show one reader another reader's likes.
    /// </remarks>
    public bool IsCacheable => CurrentUserId is null;

    /// <inheritdoc />
    public string CacheKey => $"similar_lyrics:{LyricsId}";

    /// <inheritdoc />
    public TimeSpan Ttl => TimeSpan.FromMinutes(10);

    /// <inheritdoc />
    public IReadOnlyList<string> CacheTags => [ContentCacheTags.Lyrics];
}

/// <summary>
/// Result of the <see cref="PublicGetSimilarLyricsQuery" />. Empty when no lyrics page matches
/// any branch of the waterfall — never a 404, since a missing similar-lyrics result is a normal
/// outcome, not an error.
/// </summary>
/// <param name="Lyrics">The matched similar lyrics pages, or an empty list.</param>
public record PublicGetSimilarLyricsResult(IReadOnlyList<PublicLyricsSummaryDto> Lyrics);
