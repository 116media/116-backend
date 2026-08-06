using _116.Content.Domain.Entities;

namespace _116.Tests.Fixtures.Factories.Content;

/// <summary>
/// Named aliases for <see cref="LyricsViewEventEntity" /> arrangements that three or more tests share verbatim.
/// A shape fewer tests need belongs at the call site as a builder chain, not here —
/// factory names carry the combinatorics, and combinatorics multiply.
/// </summary>
public static class LyricsViewEventFactory
{
    /// <summary>
    /// Creates a counted view event for the given lyrics page and dedup key, with a genuine
    /// full-read dwell time and scroll depth.
    /// </summary>
    public static LyricsViewEventEntity CreateCounted(Guid lyricsId, string dedupKey, Guid? userId = null) =>
        LyricsViewEventEntity.Create(
            Guid.NewGuid(),
            lyricsId,
            userId,
            dedupKey,
            ipAddress: null,
            userAgent: null,
            isCounted: true,
            dwellMs: 30_000,
            scrollDepthRatio: 1.0
        );

    /// <summary>
    /// Creates an uncounted (bounce) view event for the given lyrics page and dedup key, with
    /// negligible dwell time and scroll depth.
    /// </summary>
    public static LyricsViewEventEntity CreateUncounted(Guid lyricsId, string dedupKey, Guid? userId = null) =>
        LyricsViewEventEntity.Create(
            Guid.NewGuid(),
            lyricsId,
            userId,
            dedupKey,
            ipAddress: null,
            userAgent: null,
            isCounted: false,
            dwellMs: 300,
            scrollDepthRatio: 0.05
        );
}
