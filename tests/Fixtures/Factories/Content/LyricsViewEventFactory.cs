using _116.Content.Domain.Entities;

namespace _116.Tests.Fixtures.Factories.Content;

/// <summary>
/// Factory for quickly creating <see cref="LyricsViewEventEntity"/> instances in tests.
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

    /// <summary>
    /// Creates a view event with fully explicit fields, for tests that need precise control
    /// over dwell time, scroll depth, or the identity signals.
    /// </summary>
    public static LyricsViewEventEntity Create(
        Guid lyricsId,
        string dedupKey,
        Guid? userId = null,
        string? ipAddress = null,
        string? userAgent = null,
        bool isCounted = true,
        int dwellMs = 30_000,
        double scrollDepthRatio = 1.0
    ) =>
        LyricsViewEventEntity.Create(
            Guid.NewGuid(),
            lyricsId,
            userId,
            dedupKey,
            ipAddress,
            userAgent,
            isCounted,
            dwellMs,
            scrollDepthRatio
        );
}
