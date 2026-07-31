using _116.Content.Application.Interactions.Constants;
using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Interactions.UseCases.Public.Commands.RecordLyricsView;

/// <summary>
/// Handles the <see cref="PublicRecordLyricsViewCommand" />: stores a raw view event and
/// increments the displayed count only when both the read-time view-counting rule (spec 05)
/// and the dedup-window check pass.
/// <para>
/// The read-time rule rejects a view outright when the reported dwell time is below
/// <see cref="LyricsViewCountingConstants.MinDwellFloor" /> or the reported scroll depth is
/// below <see cref="LyricsViewCountingConstants.MinScrollDepthRatio" />. Otherwise it recomputes
/// the expected reading time server-side from the lyrics text's own word count — never trusting
/// a client-sent expectation — and requires the dwell time to reach at least
/// <c>min(expectedReadMs * MinReadTimeRatio, MaxRequiredDwell)</c>.
/// </para>
/// <para>
/// A view failing the read-time rule is not an error: it returns <c>IsSuccess: true,
/// IsCounted: false</c>, and the raw event is still persisted for tuning analysis.
/// </para>
/// </summary>
/// <param name="lyricsRepository">Repository for lyrics data access operations.</param>
/// <param name="unitOfWork">Unit of Work for managing database transactions.</param>
public class PublicRecordLyricsViewHandler(ILyricsRepository lyricsRepository, IContentUnitOfWork unitOfWork)
    : ICommandHandler<PublicRecordLyricsViewCommand, PublicRecordLyricsViewResult>
{
    /// <summary>
    /// Dedup key used when the caller exposes no identity signal at all.
    /// </summary>
    private const string UnknownDedupKey = "unknown";

    /// <inheritdoc />
    public async Task<PublicRecordLyricsViewResult> Handle(
        PublicRecordLyricsViewCommand command,
        CancellationToken cancellationToken
    )
    {
        LyricsEntity lyrics = await lyricsRepository.GetByIdOrThrowAsync(
            id: command.LyricsId,
            cancellationToken: cancellationToken
        );

        bool meetsReadTimeRule = SatisfiesReadTimeRule(
            lyricsText: lyrics.LyricsText,
            dwellMs: command.DwellMs,
            scrollDepthRatio: command.ScrollDepthRatio
        );

        string dedupKey = ResolveDedupKey(command: command);

        DateTime windowStart = DateTime.UtcNow - ViewCountingConstants.DedupWindow;

        // "unknown" is a shared bucket, not an identity — deduplicating it would make
        // unrelated signal-less viewers suppress each other, so those always count.
        bool alreadyCounted =
            dedupKey != UnknownDedupKey
            && await lyricsRepository.HasCountedViewSinceAsync(
                lyricsId: command.LyricsId,
                dedupKey: dedupKey,
                since: windowStart,
                cancellationToken: cancellationToken
            );

        bool isCounted = meetsReadTimeRule && !alreadyCounted;

        var viewEvent = LyricsViewEventEntity.Create(
            id: Guid.NewGuid(),
            lyricsId: command.LyricsId,
            userId: command.UserId,
            dedupKey: dedupKey,
            ipAddress: command.IpAddress,
            userAgent: command.UserAgent,
            isCounted: isCounted,
            dwellMs: command.DwellMs,
            scrollDepthRatio: command.ScrollDepthRatio
        );

        await lyricsRepository.AddViewEventAsync(viewEvent: viewEvent, cancellationToken: cancellationToken);

        await unitOfWork.CommitAsync(cancellationToken: cancellationToken);

        return new PublicRecordLyricsViewResult(IsSuccess: true, IsCounted: isCounted);
    }

    /// <summary>
    /// Determines whether the reported dwell time and scroll depth represent a genuine
    /// completed read of the lyrics text, per spec 05's read-time view-counting algorithm.
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

        int wordCount = lyricsText.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries).Length;
        double expectedReadMs = wordCount / LyricsViewCountingConstants.WordsPerMinute * 60_000;
        double requiredMs = Math.Min(
            expectedReadMs * LyricsViewCountingConstants.MinReadTimeRatio,
            LyricsViewCountingConstants.MaxRequiredDwell.TotalMilliseconds
        );

        return dwellMs >= requiredMs;
    }

    /// <summary>
    /// Resolves the identity surrogate the view is deduplicated against, preferring the
    /// strongest available signal: user id, then device id, then IP address.
    /// </summary>
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
