using _116.Content.Application.Editorial.Constants;
using _116.Content.Application.Shared.DTOs;
using _116.Content.Application.Shared.Mappers;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Shared.Contracts.Application.CQRS;
using MapsterMapper;

namespace _116.Content.Application.Editorial.UseCases.Public.Queries.GetArticlePromotionFeed;

/// <summary>
/// Handles the <see cref="PublicGetArticlePromotionFeedQuery" /> to build the homepage article
/// promotion feed grouped by spot priority, with gossip fallbacks for empty spots.
/// </summary>
/// <param name="articleRepository">Repository for article data access operations.</param>
/// <param name="categoryRepository">Repository for category data access operations.</param>
/// <param name="mapper">Mapster mapper for entity-to-DTO transformations.</param>
public class PublicGetArticlePromotionFeedHandler(
    IArticleRepository articleRepository,
    ICategoryRepository categoryRepository,
    IMapper mapper
) : IQueryHandler<PublicGetArticlePromotionFeedQuery, PublicGetArticlePromotionFeedResult>
{
    private const int GossipPoolSize = EditorialFeedConstants.GossipPoolSize;

    /// <inheritdoc />
    public async Task<PublicGetArticlePromotionFeedResult> Handle(
        PublicGetArticlePromotionFeedQuery query,
        CancellationToken cancellationToken
    )
    {
        CategoryEntity? gossipCategory = await categoryRepository.GetGossipCategoryAsync(cancellationToken);

        Task<IReadOnlyList<ArticleEntity>> spot1Task = articleRepository.GetActivePromotedBySpotAsync(
            spotPriority: EditorialFeedConstants.Spot1,
            cancellationToken: cancellationToken
        );

        Task<IReadOnlyList<ArticleEntity>> spot2Task = articleRepository.GetActivePromotedBySpotAsync(
            spotPriority: EditorialFeedConstants.Spot2,
            cancellationToken: cancellationToken
        );

        Task<IReadOnlyList<ArticleEntity>> spot3Task = articleRepository.GetActivePromotedBySpotAsync(
            spotPriority: EditorialFeedConstants.Spot3,
            cancellationToken: cancellationToken
        );

        await Task.WhenAll(spot1Task, spot2Task, spot3Task);

        IReadOnlyList<ArticleEntity> spot1Articles = spot1Task.Result;
        IReadOnlyList<ArticleEntity> spot2Articles = spot2Task.Result;
        IReadOnlyList<ArticleEntity> spot3Articles = spot3Task.Result;

        var usedIds = new HashSet<Guid>(spot1Articles.Concat(spot2Articles).Concat(spot3Articles).Select(a => a.Id));

        IReadOnlyList<ArticleEntity> gossipPool = gossipCategory is not null
            ? await articleRepository.GetGossipFallbackAsync(
                gossipCategoryId: gossipCategory.Id,
                limit: GossipPoolSize,
                excludeIds: usedIds,
                cancellationToken: cancellationToken
            )
            : Array.Empty<ArticleEntity>();

        var gossipQueue = new Queue<ArticleEntity>(gossipPool);

        ArticlePromotionSpotDto spot1 = BuildSimpleSpot(
            spotPriority: EditorialFeedConstants.Spot1,
            promoted: spot1Articles,
            gossipQueue: gossipQueue,
            usedIds: usedIds,
            mapper: mapper
        );

        ArticlePromotionSpotDto spot2 = BuildSimpleSpot(
            spotPriority: EditorialFeedConstants.Spot2,
            promoted: spot2Articles,
            gossipQueue: gossipQueue,
            usedIds: usedIds,
            mapper: mapper
        );

        ArticlePromotionSpot3Dto spot3 = BuildSpot3(
            promoted: spot3Articles,
            gossipQueue: gossipQueue,
            usedIds: usedIds,
            mapper: mapper
        );

        IReadOnlyList<ArticleSummaryDto> gossipStrip = BuildGossipStrip(
            gossipQueue: gossipQueue,
            stripSize: query.StripSize,
            mapper: mapper
        );

        return new PublicGetArticlePromotionFeedResult(
            Spot1: spot1,
            Spot2: spot2,
            Spot3: spot3,
            GossipStrip: gossipStrip
        );
    }

    /// <summary>
    /// Builds a simple promotion spot (1 or 2). Returns the promoted articles when available;
    /// otherwise dequeues one gossip fallback from the pool.
    /// </summary>
    /// <param name="spotPriority">The spot number (1 or 2).</param>
    /// <param name="promoted">Promoted articles assigned to this spot.</param>
    /// <param name="gossipQueue">Remaining gossip articles not yet consumed by earlier spots.</param>
    /// <param name="usedIds">Tracks all article IDs already placed in the feed to prevent duplicates.</param>
    /// <param name="mapper">Mapster mapper for entity-to-DTO transformations.</param>
    /// <returns>A <see cref="ArticlePromotionSpotDto" /> with promoted articles or a single gossip fallback.</returns>
    private static ArticlePromotionSpotDto BuildSimpleSpot(
        int spotPriority,
        IReadOnlyList<ArticleEntity> promoted,
        Queue<ArticleEntity> gossipQueue,
        HashSet<Guid> usedIds,
        IMapper mapper
    )
    {
        if (promoted.Count > 0)
        {
            return new ArticlePromotionSpotDto(
                SpotPriority: spotPriority,
                Articles: promoted.ToArticleSummaryDtos(mapper)
            );
        }

        var fallback = new List<ArticleSummaryDto>();

        if (gossipQueue.TryDequeue(out ArticleEntity? gossip))
        {
            usedIds.Add(gossip.Id);
            fallback.Add(gossip.ToArticleSummaryDto(mapper));
        }

        return new ArticlePromotionSpotDto(SpotPriority: spotPriority, Articles: fallback);
    }

    /// <summary>
    /// Builds spot 3, distributing promoted articles round-robin across two columns (a / b).
    /// Each empty column is filled with one gossip fallback.
    /// </summary>
    /// <param name="promoted">Promoted articles assigned to spot 3.</param>
    /// <param name="gossipQueue">Remaining gossip articles not yet consumed by earlier spots.</param>
    /// <param name="usedIds">Tracks all article IDs already placed in the feed to prevent duplicates.</param>
    /// <param name="mapper">Mapster mapper for entity-to-DTO transformations.</param>
    /// <returns>
    /// A <see cref="ArticlePromotionSpot3Dto" /> with two named slots (<c>"a"</c> and <c>"b"</c>),
    /// each containing at least one article.
    /// </returns>
    private static ArticlePromotionSpot3Dto BuildSpot3(
        IReadOnlyList<ArticleEntity> promoted,
        Queue<ArticleEntity> gossipQueue,
        HashSet<Guid> usedIds,
        IMapper mapper
    )
    {
        var columnA = new List<ArticleSummaryDto>();
        var columnB = new List<ArticleSummaryDto>();

        for (int i = 0; i < promoted.Count; i++)
        {
            ArticleSummaryDto dto = promoted[i].ToArticleSummaryDto(mapper);
            (i % 2 == 0 ? columnA : columnB).Add(dto);
        }

        if (columnA.Count == 0 && gossipQueue.TryDequeue(out ArticleEntity? gossipA))
        {
            usedIds.Add(gossipA.Id);
            columnA.Add(gossipA.ToArticleSummaryDto(mapper));
        }

        if (columnB.Count == 0 && gossipQueue.TryDequeue(out ArticleEntity? gossipB))
        {
            usedIds.Add(gossipB.Id);
            columnB.Add(gossipB.ToArticleSummaryDto(mapper));
        }

        var slots = new List<ArticlePromotionSlotDto>
        {
            new(Position: "a", Articles: columnA),
            new(Position: "b", Articles: columnB),
        };

        return new ArticlePromotionSpot3Dto(SpotPriority: EditorialFeedConstants.Spot3, Slots: slots);
    }

    /// <summary>
    /// Dequeues up to <paramref name="stripSize" /> articles from the remaining gossip pool and
    /// maps them to <see cref="ArticleSummaryDto" /> for the horizontal gossip strip.
    /// </summary>
    /// <param name="gossipQueue">Remaining gossip articles not yet consumed by spot fallbacks.</param>
    /// <param name="stripSize">Maximum number of articles to include in the strip.</param>
    /// <param name="mapper">Mapster mapper for entity-to-DTO transformations.</param>
    /// <returns>
    /// An ordered list of up to <paramref name="stripSize" /> gossip article summaries.
    /// May be shorter if the queue is exhausted.
    /// </returns>
    private static IReadOnlyList<ArticleSummaryDto> BuildGossipStrip(
        Queue<ArticleEntity> gossipQueue,
        int stripSize,
        IMapper mapper
    )
    {
        var strip = new List<ArticleSummaryDto>();

        while (strip.Count < stripSize && gossipQueue.TryDequeue(out ArticleEntity? article))
        {
            strip.Add(article.ToArticleSummaryDto(mapper));
        }

        return strip;
    }
}
