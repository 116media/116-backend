using _116.Content.Application.Editorial.Constants;
using _116.Content.Application.Shared.DTOs;
using _116.Content.Application.Shared.Mappers;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Shared.Contracts.Application.CQRS;
using MapsterMapper;

namespace _116.Content.Application.Editorial.UseCases.Public.Queries.GetVideoPromotionFeed;

/// <summary>
/// Handles the <see cref="PublicGetVideoPromotionFeedQuery" /> to build the homepage video
/// promotion feed grouped by spot priority, with randomly selected free video fallbacks for
/// empty spots.
/// </summary>
/// <param name="videoRepository">Repository for video data access operations.</param>
/// <param name="mapper">Mapster mapper for entity-to-DTO transformations.</param>
public class PublicGetVideoPromotionFeedHandler(IVideoRepository videoRepository, IMapper mapper)
    : IQueryHandler<PublicGetVideoPromotionFeedQuery, PublicGetVideoPromotionFeedResult>
{
    private const int FreeVideoPoolSize = EditorialFeedConstants.FreeVideoPoolSize;

    /// <inheritdoc />
    public async Task<PublicGetVideoPromotionFeedResult> Handle(
        PublicGetVideoPromotionFeedQuery query,
        CancellationToken cancellationToken
    )
    {
        Task<IReadOnlyList<VideoEntity>> spot1Task = videoRepository.GetActivePromotedBySpotAsync(
            spotPriority: EditorialFeedConstants.Spot1,
            cancellationToken: cancellationToken
        );

        Task<IReadOnlyList<VideoEntity>> spot2Task = videoRepository.GetActivePromotedBySpotAsync(
            spotPriority: EditorialFeedConstants.Spot2,
            cancellationToken: cancellationToken
        );

        Task<IReadOnlyList<VideoEntity>> spot3Task = videoRepository.GetActivePromotedBySpotAsync(
            spotPriority: EditorialFeedConstants.Spot3,
            cancellationToken: cancellationToken
        );

        await Task.WhenAll(spot1Task, spot2Task, spot3Task);

        IReadOnlyList<VideoEntity> spot1Videos = spot1Task.Result;
        IReadOnlyList<VideoEntity> spot2Videos = spot2Task.Result;
        IReadOnlyList<VideoEntity> spot3Videos = spot3Task.Result;

        var usedIds = new HashSet<Guid>(spot1Videos.Concat(spot2Videos).Concat(spot3Videos).Select(v => v.Id));

        IReadOnlyList<VideoEntity> freePool = await videoRepository.GetFreeVideosAsync(
            limit: FreeVideoPoolSize,
            excludeIds: usedIds,
            cancellationToken: cancellationToken
        );

        IReadOnlyList<VideoEntity> shuffledPool = freePool.OrderBy(_ => Guid.NewGuid()).ToList();
        var freeQueue = new Queue<VideoEntity>(shuffledPool);

        VideoPromotionSpotDto spot1 = BuildSimpleSpot(
            spotPriority: EditorialFeedConstants.Spot1,
            promoted: spot1Videos,
            freeQueue: freeQueue,
            usedIds: usedIds,
            mapper: mapper
        );

        VideoPromotionSpotDto spot2 = BuildSimpleSpot(
            spotPriority: EditorialFeedConstants.Spot2,
            promoted: spot2Videos,
            freeQueue: freeQueue,
            usedIds: usedIds,
            mapper: mapper
        );

        VideoPromotionSpot3Dto spot3 = BuildSpot3(
            promoted: spot3Videos,
            freeQueue: freeQueue,
            usedIds: usedIds,
            mapper: mapper
        );

        IReadOnlyList<VideoSummaryDto> freeVideoStrip = BuildFreeVideoStrip(
            freeQueue: freeQueue,
            stripSize: query.StripSize,
            mapper: mapper
        );

        return new PublicGetVideoPromotionFeedResult(
            Spot1: spot1,
            Spot2: spot2,
            Spot3: spot3,
            FreeVideoStrip: freeVideoStrip
        );
    }

    /// <summary>
    /// Builds a simple promotion spot (1 or 2). Returns the promoted videos when available;
    /// otherwise dequeues one free video fallback from the pool.
    /// </summary>
    /// <param name="spotPriority">The spot number (1 or 2).</param>
    /// <param name="promoted">Promoted videos assigned to this spot.</param>
    /// <param name="freeQueue">Remaining free videos not yet consumed by earlier spots.</param>
    /// <param name="usedIds">Tracks all video IDs already placed in the feed to prevent duplicates.</param>
    /// <param name="mapper">Mapster mapper for entity-to-DTO transformations.</param>
    /// <returns>A <see cref="VideoPromotionSpotDto" /> with promoted videos or a single free video fallback.</returns>
    private static VideoPromotionSpotDto BuildSimpleSpot(
        int spotPriority,
        IReadOnlyList<VideoEntity> promoted,
        Queue<VideoEntity> freeQueue,
        HashSet<Guid> usedIds,
        IMapper mapper
    )
    {
        if (promoted.Count > 0)
        {
            return new VideoPromotionSpotDto(SpotPriority: spotPriority, Videos: promoted.ToVideoSummaryDtos(mapper));
        }

        var fallback = new List<VideoSummaryDto>();

        if (freeQueue.TryDequeue(out VideoEntity? freeVideo))
        {
            usedIds.Add(freeVideo.Id);
            fallback.Add(freeVideo.ToVideoSummaryDto(mapper));
        }

        return new VideoPromotionSpotDto(SpotPriority: spotPriority, Videos: fallback);
    }

    /// <summary>
    /// Builds spot 3, distributing promoted videos round-robin across two columns (a / b).
    /// Each empty column is filled with one free video fallback.
    /// </summary>
    /// <param name="promoted">Promoted videos assigned to spot 3.</param>
    /// <param name="freeQueue">Remaining free videos not yet consumed by earlier spots.</param>
    /// <param name="usedIds">Tracks all video IDs already placed in the feed to prevent duplicates.</param>
    /// <param name="mapper">Mapster mapper for entity-to-DTO transformations.</param>
    /// <returns>
    /// A <see cref="VideoPromotionSpot3Dto" /> with two named slots (<c>"a"</c> and <c>"b"</c>),
    /// each containing at least one video.
    /// </returns>
    private static VideoPromotionSpot3Dto BuildSpot3(
        IReadOnlyList<VideoEntity> promoted,
        Queue<VideoEntity> freeQueue,
        HashSet<Guid> usedIds,
        IMapper mapper
    )
    {
        var columnA = new List<VideoSummaryDto>();
        var columnB = new List<VideoSummaryDto>();

        for (int i = 0; i < promoted.Count; i++)
        {
            VideoSummaryDto dto = promoted[i].ToVideoSummaryDto(mapper);
            (i % 2 == 0 ? columnA : columnB).Add(dto);
        }

        if (columnA.Count == 0 && freeQueue.TryDequeue(out VideoEntity? freeA))
        {
            usedIds.Add(freeA.Id);
            columnA.Add(freeA.ToVideoSummaryDto(mapper));
        }

        if (columnB.Count == 0 && freeQueue.TryDequeue(out VideoEntity? freeB))
        {
            usedIds.Add(freeB.Id);
            columnB.Add(freeB.ToVideoSummaryDto(mapper));
        }

        var slots = new List<VideoPromotionSlotDto>
        {
            new(Position: "a", Videos: columnA),
            new(Position: "b", Videos: columnB),
        };

        return new VideoPromotionSpot3Dto(SpotPriority: EditorialFeedConstants.Spot3, Slots: slots);
    }

    /// <summary>
    /// Dequeues up to <paramref name="stripSize" /> videos from the remaining free pool and
    /// maps them to <see cref="VideoSummaryDto" /> for the horizontal free-video strip.
    /// </summary>
    /// <param name="freeQueue">Remaining free videos not yet consumed by spot fallbacks.</param>
    /// <param name="stripSize">Maximum number of videos to include in the strip.</param>
    /// <param name="mapper">Mapster mapper for entity-to-DTO transformations.</param>
    /// <returns>
    /// An ordered list of up to <paramref name="stripSize" /> free video summaries.
    /// May be shorter if the queue is exhausted.
    /// </returns>
    private static IReadOnlyList<VideoSummaryDto> BuildFreeVideoStrip(
        Queue<VideoEntity> freeQueue,
        int stripSize,
        IMapper mapper
    )
    {
        var strip = new List<VideoSummaryDto>();

        while (strip.Count < stripSize && freeQueue.TryDequeue(out VideoEntity? video))
        {
            strip.Add(video.ToVideoSummaryDto(mapper));
        }

        return strip;
    }
}
