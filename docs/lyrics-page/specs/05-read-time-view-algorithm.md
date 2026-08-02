# Spec 05 — Read-Time View-Counting Algorithm

`ShortVideoEntity`'s view counter counts on page load, deduped per identity per 24h window
(`ViewCountingConstants.DedupWindow`). That's the right rule for a video (playing it *is* the
engagement signal). For a lyrics page, a page load proves nothing about whether the visitor
actually read the lyrics. This spec extends `PublicRecordLyricsViewCommand` (spec 04) with a
read-time gate before a view is allowed to increment `ViewCount`.

## Command additions

```csharp
/// <summary>
/// Command to record a view of a lyrics page. A view increments the displayed count only
/// when the read-time counting rule (see <see cref="LyricsViewCountingConstants" />) is
/// satisfied, in addition to the existing dedup-window check.
/// </summary>
/// <param name="LyricsId">The lyrics page that was viewed.</param>
/// <param name="UserId">The identity user UUID of the viewer, or null if anonymous.</param>
/// <param name="DeviceId">The client-supplied device identifier, used as a dedup fallback.</param>
/// <param name="IpAddress">The caller's IP address, used as a dedup fallback and fraud signal.</param>
/// <param name="UserAgent">The caller's User-Agent header, kept as a fraud signal.</param>
/// <param name="DwellMs">
/// Total foreground dwell time on the lyrics body, in milliseconds, as measured client-side.
/// </param>
/// <param name="ScrollDepthRatio">
/// Maximum scroll coverage of the lyrics text (0.0–1.0), as measured client-side.
/// </param>
public record PublicRecordLyricsViewCommand(
    Guid LyricsId,
    Guid? UserId,
    string? DeviceId,
    string? IpAddress,
    string? UserAgent,
    int DwellMs,
    double ScrollDepthRatio
) : ICommand<PublicRecordLyricsViewResult>;

/// <summary>
/// Result of the <see cref="PublicRecordLyricsViewCommand" />.
/// </summary>
/// <param name="IsSuccess">Always true — a view that doesn't meet the counting rule is not an error.</param>
/// <param name="IsCounted">Whether this view incremented the displayed count.</param>
public record PublicRecordLyricsViewResult(bool IsSuccess, bool IsCounted);
```

## Tuning constants

Mirrors `ViewCountingConstants`'s own shape (`Application/Interactions/Constants/`), as a sibling
class rather than adding fields to the shared one — these three numbers are lyrics-specific and
have no meaning for short videos:

```csharp
/// <summary>
/// Tuning knobs for the lyrics read-time view-counting rule: how fast a visitor is assumed
/// to read lyrics, and the minimum engagement thresholds a view must clear before it counts.
/// </summary>
public static class LyricsViewCountingConstants
{
    /// <summary>
    /// Assumed reading speed in words per minute. Deliberately below typical silent-reading
    /// speed (~200–250 wpm) — lyrics are read with pauses, repeated choruses, and are often
    /// mentally sung along to rather than read straight through once.
    /// </summary>
    public const double WordsPerMinute = 130.0;

    /// <summary>
    /// Minimum fraction of the expected reading time the visitor must have spent, in the
    /// foreground, before a view counts.
    /// </summary>
    public const double MinReadTimeRatio = 0.6;

    /// <summary>
    /// Upper cap on the minimum-dwell-time requirement, regardless of song length, so a long
    /// song does not require an unreasonably long minimum dwell.
    /// </summary>
    public static readonly TimeSpan MaxRequiredDwell = TimeSpan.FromSeconds(60);

    /// <summary>
    /// Absolute floor below which a view never counts, regardless of the ratio checks —
    /// rejects instantaneous or scripted requests outright.
    /// </summary>
    public static readonly TimeSpan MinDwellFloor = TimeSpan.FromSeconds(1.5);

    /// <summary>
    /// Minimum scroll coverage of the lyrics text required for a view to count.
    /// </summary>
    public const double MinScrollDepthRatio = 0.7;
}
```

## Handler

Extends `PublicRecordShortVideoViewHandler`'s exact dedup-window logic with the read-time gate
applied first:

```csharp
public class PublicRecordLyricsViewHandler(
    ILyricsRepository lyricsRepository, IContentUnitOfWork unitOfWork
) : ICommandHandler<PublicRecordLyricsViewCommand, PublicRecordLyricsViewResult>
{
    private const string UnknownDedupKey = "unknown";

    public async Task<PublicRecordLyricsViewResult> Handle(
        PublicRecordLyricsViewCommand command, CancellationToken cancellationToken)
    {
        LyricsEntity lyrics = await lyricsRepository.GetByIdOrThrowAsync(command.LyricsId, cancellationToken);

        bool meetsReadTimeRule = SatisfiesReadTimeRule(lyrics.LyricsText, command.DwellMs, command.ScrollDepthRatio);

        string dedupKey = ResolveDedupKey(command);
        DateTime windowStart = DateTime.UtcNow - ViewCountingConstants.DedupWindow;

        bool alreadyCounted =
            dedupKey != UnknownDedupKey
            && await lyricsRepository.HasCountedViewSinceAsync(
                lyricsId: command.LyricsId, dedupKey: dedupKey, since: windowStart,
                cancellationToken: cancellationToken);

        bool isCounted = meetsReadTimeRule && !alreadyCounted;

        var viewEvent = LyricsViewEventEntity.Create(
            id: Guid.NewGuid(), lyricsId: command.LyricsId, userId: command.UserId,
            dedupKey: dedupKey, ipAddress: command.IpAddress, userAgent: command.UserAgent,
            isCounted: isCounted, dwellMs: command.DwellMs, scrollDepthRatio: command.ScrollDepthRatio);

        await lyricsRepository.AddViewEventAsync(viewEvent, cancellationToken);

        if (isCounted)
        {
            lyrics.IncrementViewCount();
            lyricsRepository.Update(lyrics);
        }

        await unitOfWork.CommitAsync(cancellationToken);

        return new PublicRecordLyricsViewResult(IsSuccess: true, IsCounted: isCounted);
    }

    /// <summary>
    /// Determines whether a view's client-reported dwell time and scroll depth are
    /// consistent with the visitor having actually read the lyrics, given the song's own
    /// text length. The expected reading time is always recomputed server-side from the
    /// stored text — a client-sent expected value is never trusted.
    /// </summary>
    private static bool SatisfiesReadTimeRule(string lyricsText, int dwellMs, double scrollDepthRatio)
    {
        if (dwellMs < LyricsViewCountingConstants.MinDwellFloor.TotalMilliseconds)
        {
            return false;
        }

        if (scrollDepthRatio < LyricsViewCountingConstants.MinScrollDepthRatio)
        {
            return false;
        }

        int wordCount = lyricsText.Split(
            separator: (char[]?)null,
            options: StringSplitOptions.RemoveEmptyEntries
        ).Length;

        double expectedReadMs = wordCount / LyricsViewCountingConstants.WordsPerMinute * 60_000;
        double requiredMs = Math.Min(
            expectedReadMs * LyricsViewCountingConstants.MinReadTimeRatio,
            LyricsViewCountingConstants.MaxRequiredDwell.TotalMilliseconds
        );

        return dwellMs >= requiredMs;
    }

    private static string ResolveDedupKey(PublicRecordLyricsViewCommand command)
    {
        if (command.UserId is Guid userId)
        {
            return $"user:{userId}";
        }

        if (!string.IsNullOrWhiteSpace(command.DeviceId))
        {
            return $"device:{command.DeviceId}";
        }

        if (!string.IsNullOrWhiteSpace(command.IpAddress))
        {
            return $"ip:{command.IpAddress}";
        }

        return UnknownDedupKey;
    }
}
```

A view that fails `SatisfiesReadTimeRule` is **not an error** — the request still returns
`IsSuccess: true, IsCounted: false`, and the raw event is still persisted (with its `DwellMs`/
`ScrollDepthRatio`) for later tuning analysis. Only the counted branch touches `ViewCount`.

## Anti-gaming notes

- The server never trusts a client-sent "expected reading time" — it's recomputed from
  `lyrics.LyricsText`'s own word count every time, so a client can't shortcut the check by
  reporting an inflated dwell time relative to a fabricated expectation.
- `MinDwellFloor` (1.5s) rejects instantaneous/scripted requests regardless of how the ratio math
  computes for a very short song.
- The three tunable numbers (`WordsPerMinute`, `MinReadTimeRatio`, `MinScrollDepthRatio`) live in
  one named constants class specifically so they can be adjusted from analysis of the persisted
  `DwellMs`/`ScrollDepthRatio` data without touching handler logic.

## Task checklist

- [x] `PublicRecordLyricsViewCommand` gains `DwellMs`, `ScrollDepthRatio`
- [x] `LyricsViewCountingConstants` (separate from `ViewCountingConstants`)
- [x] `PublicRecordLyricsViewHandler.SatisfiesReadTimeRule` — server-recomputed expected reading
  time, never trusting a client-sent expectation
- [x] `PublicRecordLyricsViewCommand`/`Validator`: `DwellMs >= 0`, `ScrollDepthRatio` in `[0, 1]`
- [x] Integration tests: a view with `dwellMs` below the floor never counts even with full scroll
  depth; a view with high scroll but insufficient dwell for a long song doesn't count; a
  short song with near-zero expected time still requires the absolute floor; a genuine
  full read counts exactly once per dedup window; a view failing the rule still returns
  `IsSuccess: true, IsCounted: false` and the raw event is persisted for later tuning analysis
- [x] Unit tests for `SatisfiesReadTimeRule` covering: very short lyrics, very long lyrics,
  boundary values at each of the four constants

**Verification, 2026-07-31**: `dotnet build` clean; covered by the same Lyrics-scoped test run as
specs 04/06 (376/376 unit, 177/177 integration, zero skips) since all three shipped in the same
implementation pass.
