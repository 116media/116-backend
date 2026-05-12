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
    private const int GossipPoolSize = 20;
    private const int GossipStripSize = 3;

    /// <inheritdoc />
    public async Task<PublicGetArticlePromotionFeedResult> Handle(
        PublicGetArticlePromotionFeedQuery query,
        CancellationToken cancellationToken
    )
    {
        CategoryEntity? gossipCategory = await categoryRepository.GetGossipCategoryAsync(cancellationToken);

        Task<IReadOnlyList<ArticleEntity>> spot1Task = articleRepository.GetActivePromotedBySpotAsync(
            spotPriority: 1,
            cancellationToken: cancellationToken
        );

        Task<IReadOnlyList<ArticleEntity>> spot2Task = articleRepository.GetActivePromotedBySpotAsync(
            spotPriority: 2,
            cancellationToken: cancellationToken
        );

        Task<IReadOnlyList<ArticleEntity>> spot3Task = articleRepository.GetActivePromotedBySpotAsync(
            spotPriority: 3,
            cancellationToken: cancellationToken
        );

        await Task.WhenAll(spot1Task, spot2Task, spot3Task);

        IReadOnlyList<ArticleEntity> spot1Articles = spot1Task.Result;
        IReadOnlyList<ArticleEntity> spot2Articles = spot2Task.Result;
        IReadOnlyList<ArticleEntity> spot3Articles = spot3Task.Result;

        var usedIds = new HashSet<Guid>();

        foreach (ArticleEntity article in spot1Articles)
        {
            usedIds.Add(article.Id);
        }

        foreach (ArticleEntity article in spot2Articles)
        {
            usedIds.Add(article.Id);
        }

        foreach (ArticleEntity article in spot3Articles)
        {
            usedIds.Add(article.Id);
        }

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
            spotPriority: 1,
            promoted: spot1Articles,
            gossipQueue: gossipQueue,
            usedIds: usedIds,
            mapper: mapper
        );

        ArticlePromotionSpotDto spot2 = BuildSimpleSpot(
            spotPriority: 2,
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

        IReadOnlyList<ArticleSummaryDto> gossipStrip = BuildGossipStrip(gossipQueue: gossipQueue, mapper: mapper);

        return new PublicGetArticlePromotionFeedResult(
            Spot1: spot1,
            Spot2: spot2,
            Spot3: spot3,
            GossipStrip: gossipStrip
        );
    }

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

            if (i % 2 == 0)
            {
                columnA.Add(dto);
            }
            else
            {
                columnB.Add(dto);
            }
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

        return new ArticlePromotionSpot3Dto(SpotPriority: 3, Slots: slots);
    }

    private static IReadOnlyList<ArticleSummaryDto> BuildGossipStrip(Queue<ArticleEntity> gossipQueue, IMapper mapper)
    {
        var strip = new List<ArticleSummaryDto>();

        while (strip.Count < GossipStripSize && gossipQueue.TryDequeue(out ArticleEntity? article))
        {
            strip.Add(article.ToArticleSummaryDto(mapper));
        }

        return strip;
    }
}
