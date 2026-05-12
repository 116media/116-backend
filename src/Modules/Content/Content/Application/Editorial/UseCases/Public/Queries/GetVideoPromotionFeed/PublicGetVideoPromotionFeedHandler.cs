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
    private const int FreeVideoPoolSize = 20;
    private const int FreeVideoStripSize = 3;

    /// <inheritdoc />
    public async Task<PublicGetVideoPromotionFeedResult> Handle(
        PublicGetVideoPromotionFeedQuery query,
        CancellationToken cancellationToken
    )
    {
        Task<IReadOnlyList<VideoEntity>> spot1Task = videoRepository.GetActivePromotedBySpotAsync(
            spotPriority: 1,
            cancellationToken: cancellationToken
        );

        Task<IReadOnlyList<VideoEntity>> spot2Task = videoRepository.GetActivePromotedBySpotAsync(
            spotPriority: 2,
            cancellationToken: cancellationToken
        );

        Task<IReadOnlyList<VideoEntity>> spot3Task = videoRepository.GetActivePromotedBySpotAsync(
            spotPriority: 3,
            cancellationToken: cancellationToken
        );

        await Task.WhenAll(spot1Task, spot2Task, spot3Task);

        IReadOnlyList<VideoEntity> spot1Videos = spot1Task.Result;
        IReadOnlyList<VideoEntity> spot2Videos = spot2Task.Result;
        IReadOnlyList<VideoEntity> spot3Videos = spot3Task.Result;

        var usedIds = new HashSet<Guid>();

        foreach (VideoEntity video in spot1Videos)
        {
            usedIds.Add(video.Id);
        }

        foreach (VideoEntity video in spot2Videos)
        {
            usedIds.Add(video.Id);
        }

        foreach (VideoEntity video in spot3Videos)
        {
            usedIds.Add(video.Id);
        }

        IReadOnlyList<VideoEntity> freePool = await videoRepository.GetFreeVideosAsync(
            limit: FreeVideoPoolSize,
            excludeIds: usedIds,
            cancellationToken: cancellationToken
        );

        IReadOnlyList<VideoEntity> shuffledPool = freePool.OrderBy(_ => Guid.NewGuid()).ToList();
        var freeQueue = new Queue<VideoEntity>(shuffledPool);

        VideoPromotionSpotDto spot1 = BuildSimpleSpot(
            spotPriority: 1,
            promoted: spot1Videos,
            freeQueue: freeQueue,
            usedIds: usedIds,
            mapper: mapper
        );

        VideoPromotionSpotDto spot2 = BuildSimpleSpot(
            spotPriority: 2,
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

        IReadOnlyList<VideoSummaryDto> freeVideoStrip = BuildFreeVideoStrip(freeQueue: freeQueue, mapper: mapper);

        return new PublicGetVideoPromotionFeedResult(
            Spot1: spot1,
            Spot2: spot2,
            Spot3: spot3,
            FreeVideoStrip: freeVideoStrip
        );
    }

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

            if (i % 2 == 0)
            {
                columnA.Add(dto);
            }
            else
            {
                columnB.Add(dto);
            }
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

        return new VideoPromotionSpot3Dto(SpotPriority: 3, Slots: slots);
    }

    private static IReadOnlyList<VideoSummaryDto> BuildFreeVideoStrip(Queue<VideoEntity> freeQueue, IMapper mapper)
    {
        var strip = new List<VideoSummaryDto>();

        while (strip.Count < FreeVideoStripSize && freeQueue.TryDequeue(out VideoEntity? video))
        {
            strip.Add(video.ToVideoSummaryDto(mapper));
        }

        return strip;
    }
}
