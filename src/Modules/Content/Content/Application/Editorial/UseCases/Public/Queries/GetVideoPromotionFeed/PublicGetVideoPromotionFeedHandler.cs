using _116.Content.Application.Editorial.Constants;
using _116.Content.Application.Shared.DTOs;
using _116.Content.Application.Shared.Mappers;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Core.Contracts.Application.Services;
using _116.Shared.Contracts.Application.CQRS;
using MapsterMapper;

namespace _116.Content.Application.Editorial.UseCases.Public.Queries.GetVideoPromotionFeed;

/// <summary>
/// Handles the <see cref="PublicGetVideoPromotionFeedQuery" /> to build the homepage video
/// promotion feed grouped by spot priority, with randomly selected free video fallbacks for
/// empty spots.
/// </summary>
/// <param name="videoRepository">Repository for video data access operations.</param>
/// <param name="fileStorage">Core's storage contract.</param>
/// <param name="mapper">Mapster mapper for entity-to-DTO transformations.</param>
public class PublicGetVideoPromotionFeedHandler(
    IVideoRepository videoRepository,
    IFileStorageService fileStorage,
    IMapper mapper
) : IQueryHandler<PublicGetVideoPromotionFeedQuery, PublicGetVideoPromotionFeedResult>
{
    private const int FreeVideoPoolSize = EditorialFeedConstants.FreeVideoPoolSize;

    /// <inheritdoc />
    public async Task<PublicGetVideoPromotionFeedResult> Handle(
        PublicGetVideoPromotionFeedQuery query,
        CancellationToken cancellationToken
    )
    {
        IReadOnlyList<VideoEntity> spot1Videos = await videoRepository.GetActivePromotedBySpotAsync(
            spotPriority: EditorialFeedConstants.Spot1,
            cancellationToken: cancellationToken
        );

        IReadOnlyList<VideoEntity> spot2Videos = await videoRepository.GetActivePromotedBySpotAsync(
            spotPriority: EditorialFeedConstants.Spot2,
            cancellationToken: cancellationToken
        );

        IReadOnlyList<VideoEntity> spot3Videos = await videoRepository.GetActivePromotedBySpotAsync(
            spotPriority: EditorialFeedConstants.Spot3,
            cancellationToken: cancellationToken
        );

        var usedIds = new HashSet<Guid>(spot1Videos.Concat(spot2Videos).Concat(spot3Videos).Select(v => v.Id));

        IReadOnlyList<VideoEntity> freePool = await videoRepository.GetFreeVideosAsync(
            limit: FreeVideoPoolSize,
            excludeIds: usedIds,
            cancellationToken: cancellationToken
        );

        IReadOnlyList<VideoEntity> shuffledPool = freePool.OrderBy(_ => Guid.NewGuid()).ToList();
        var freeQueue = new Queue<VideoEntity>(shuffledPool);

        VideoPromotionSpotDto spot1 = await BuildSimpleSpotAsync(
            spotPriority: EditorialFeedConstants.Spot1,
            promoted: spot1Videos,
            freeQueue: freeQueue,
            usedIds: usedIds,
            mapper: mapper,
            fileStorage: fileStorage,
            cancellationToken: cancellationToken
        );

        VideoPromotionSpotDto spot2 = await BuildSimpleSpotAsync(
            spotPriority: EditorialFeedConstants.Spot2,
            promoted: spot2Videos,
            freeQueue: freeQueue,
            usedIds: usedIds,
            mapper: mapper,
            fileStorage: fileStorage,
            cancellationToken: cancellationToken
        );

        VideoPromotionSpot3Dto spot3 = await BuildSpot3Async(
            promoted: spot3Videos,
            freeQueue: freeQueue,
            usedIds: usedIds,
            mapper: mapper,
            fileStorage: fileStorage,
            cancellationToken: cancellationToken
        );

        IReadOnlyList<PublicVideoSummaryDto> freeVideoStrip = await BuildFreeVideoStripAsync(
            freeQueue: freeQueue,
            stripSize: query.StripSize,
            mapper: mapper,
            fileStorage: fileStorage,
            cancellationToken: cancellationToken
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
    /// <param name="fileStorage">Core's storage contract.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A <see cref="VideoPromotionSpotDto" /> with promoted videos or a single free video fallback.</returns>
    private static async Task<VideoPromotionSpotDto> BuildSimpleSpotAsync(
        int spotPriority,
        IReadOnlyList<VideoEntity> promoted,
        Queue<VideoEntity> freeQueue,
        HashSet<Guid> usedIds,
        IMapper mapper,
        IFileStorageService fileStorage,
        CancellationToken cancellationToken
    )
    {
        if (promoted.Count > 0)
        {
            IReadOnlyList<PublicVideoSummaryDto> dtos = await promoted.ToPublicVideoSummaryDtosAsync(
                fileStorage,
                cancellationToken
            );
            return new VideoPromotionSpotDto(SpotPriority: spotPriority, Videos: dtos);
        }

        var fallback = new List<PublicVideoSummaryDto>();

        if (freeQueue.TryDequeue(out VideoEntity? freeVideo))
        {
            usedIds.Add(freeVideo.Id);
            fallback.Add(await freeVideo.ToPublicVideoSummaryDtoAsync(fileStorage, cancellationToken));
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
    /// <param name="fileStorage">Core's storage contract.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>
    /// A <see cref="VideoPromotionSpot3Dto" /> with two named slots (<c>"a"</c> and <c>"b"</c>),
    /// each containing at least one video.
    /// </returns>
    private static async Task<VideoPromotionSpot3Dto> BuildSpot3Async(
        IReadOnlyList<VideoEntity> promoted,
        Queue<VideoEntity> freeQueue,
        HashSet<Guid> usedIds,
        IMapper mapper,
        IFileStorageService fileStorage,
        CancellationToken cancellationToken
    )
    {
        var columnA = new List<PublicVideoSummaryDto>();
        var columnB = new List<PublicVideoSummaryDto>();

        for (int i = 0; i < promoted.Count; i++)
        {
            PublicVideoSummaryDto dto = await promoted[i].ToPublicVideoSummaryDtoAsync(fileStorage, cancellationToken);
            (i % 2 == 0 ? columnA : columnB).Add(dto);
        }

        if (columnA.Count == 0 && freeQueue.TryDequeue(out VideoEntity? freeA))
        {
            usedIds.Add(freeA.Id);
            columnA.Add(await freeA.ToPublicVideoSummaryDtoAsync(fileStorage, cancellationToken));
        }

        if (columnB.Count == 0 && freeQueue.TryDequeue(out VideoEntity? freeB))
        {
            usedIds.Add(freeB.Id);
            columnB.Add(await freeB.ToPublicVideoSummaryDtoAsync(fileStorage, cancellationToken));
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
    /// maps them to <see cref="PublicVideoSummaryDto" /> for the horizontal free-video strip.
    /// </summary>
    /// <param name="freeQueue">Remaining free videos not yet consumed by spot fallbacks.</param>
    /// <param name="stripSize">Maximum number of videos to include in the strip.</param>
    /// <param name="mapper">Mapster mapper for entity-to-DTO transformations.</param>
    /// <param name="fileStorage">Core's storage contract.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>
    /// An ordered list of up to <paramref name="stripSize" /> free video summaries.
    /// May be shorter if the queue is exhausted.
    /// </returns>
    private static async Task<IReadOnlyList<PublicVideoSummaryDto>> BuildFreeVideoStripAsync(
        Queue<VideoEntity> freeQueue,
        int stripSize,
        IMapper mapper,
        IFileStorageService fileStorage,
        CancellationToken cancellationToken
    )
    {
        var strip = new List<PublicVideoSummaryDto>();

        while (strip.Count < stripSize && freeQueue.TryDequeue(out VideoEntity? video))
        {
            strip.Add(await video.ToPublicVideoSummaryDtoAsync(fileStorage, cancellationToken));
        }

        return strip;
    }
}
