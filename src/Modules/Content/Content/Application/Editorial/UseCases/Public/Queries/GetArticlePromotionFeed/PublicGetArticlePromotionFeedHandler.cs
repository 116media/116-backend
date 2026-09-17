using _116.Content.Application.Editorial.Constants;
using _116.Content.Application.Shared.DTOs;
using _116.Content.Application.Shared.Mappers;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Core.Contracts.Application.Services;
using _116.Shared.Contracts.Application.CQRS;
using MapsterMapper;

namespace _116.Content.Application.Editorial.UseCases.Public.Queries.GetArticlePromotionFeed;

/// <summary>
/// Handles the <see cref="PublicGetArticlePromotionFeedQuery" /> to build the homepage article
/// promotion feed grouped by spot priority, with gossip fallbacks for empty spots.
/// </summary>
/// <param name="articleRepository">Repository for article data access operations.</param>
/// <param name="articleInteractionRepository">Repository for article interaction data access operations.</param>
/// <param name="categoryRepository">Repository for category data access operations.</param>
/// <param name="fileStorage">Core's storage contract.</param>
/// <param name="mapper">Mapster mapper for entity-to-DTO transformations.</param>
public class PublicGetArticlePromotionFeedHandler(
    IArticleRepository articleRepository,
    IArticleInteractionRepository articleInteractionRepository,
    ICategoryRepository categoryRepository,
    IFileStorageService fileStorage,
    IMapper mapper,
    IContentLookupFactory contentLookupFactory
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

        IReadOnlyList<ArticleEntity> spot1Articles = await articleRepository.GetActivePromotedBySpotAsync(
            spotPriority: EditorialFeedConstants.Spot1,
            cancellationToken: cancellationToken
        );

        IReadOnlyList<ArticleEntity> spot2Articles = await articleRepository.GetActivePromotedBySpotAsync(
            spotPriority: EditorialFeedConstants.Spot2,
            cancellationToken: cancellationToken
        );

        IReadOnlyList<ArticleEntity> spot3Articles = await articleRepository.GetActivePromotedBySpotAsync(
            spotPriority: EditorialFeedConstants.Spot3,
            cancellationToken: cancellationToken
        );

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

        List<Guid> allFeedIds = spot1Articles
            .Concat(spot2Articles)
            .Concat(spot3Articles)
            .Concat(gossipPool)
            .Select(article => article.Id)
            .Distinct()
            .ToList();

        (IReadOnlySet<Guid> liked, IReadOnlySet<Guid> bookmarked) =
            await articleInteractionRepository.GetLikedAndBookmarkedIdsAsync(
                currentUserId: query.CurrentUserId,
                articleIds: allFeedIds,
                cancellationToken: cancellationToken
            );

        // One batch for every article across the spots and the gossip strip.
        ContentLookups lookups = await contentLookupFactory.ResolveForArticlesAsync(
            [.. spot1Articles, .. spot2Articles, .. spot3Articles, .. gossipPool],
            cancellationToken
        );

        ArticlePromotionSpotDto spot1 = await BuildSimpleSpotAsync(
            spotPriority: EditorialFeedConstants.Spot1,
            promoted: spot1Articles,
            gossipQueue: gossipQueue,
            usedIds: usedIds,
            mapper: mapper,
            lookups: lookups,
            fileStorage: fileStorage,
            likedArticleIds: liked,
            bookmarkedArticleIds: bookmarked,
            cancellationToken: cancellationToken
        );

        ArticlePromotionSpotDto spot2 = await BuildSimpleSpotAsync(
            spotPriority: EditorialFeedConstants.Spot2,
            promoted: spot2Articles,
            gossipQueue: gossipQueue,
            usedIds: usedIds,
            mapper: mapper,
            lookups: lookups,
            fileStorage: fileStorage,
            likedArticleIds: liked,
            bookmarkedArticleIds: bookmarked,
            cancellationToken: cancellationToken
        );

        ArticlePromotionSpot3Dto spot3 = await BuildSpot3Async(
            promoted: spot3Articles,
            gossipQueue: gossipQueue,
            usedIds: usedIds,
            mapper: mapper,
            lookups: lookups,
            fileStorage: fileStorage,
            likedArticleIds: liked,
            bookmarkedArticleIds: bookmarked,
            cancellationToken: cancellationToken
        );

        IReadOnlyList<PublicArticleSummaryDto> gossipStrip = await BuildGossipStripAsync(
            gossipQueue: gossipQueue,
            stripSize: query.StripSize,
            mapper: mapper,
            lookups: lookups,
            fileStorage: fileStorage,
            likedArticleIds: liked,
            bookmarkedArticleIds: bookmarked,
            cancellationToken: cancellationToken
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
    /// <param name="lookups">The categories the cards name, resolved for the whole feed.</param>
    /// <param name="fileStorage">Core's storage contract.</param>
    /// <param name="likedArticleIds">Ids the current user has liked.</param>
    /// <param name="bookmarkedArticleIds">Ids the current user has bookmarked.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A <see cref="ArticlePromotionSpotDto" /> with promoted articles or a single gossip fallback.</returns>
    private static async Task<ArticlePromotionSpotDto> BuildSimpleSpotAsync(
        int spotPriority,
        IReadOnlyList<ArticleEntity> promoted,
        Queue<ArticleEntity> gossipQueue,
        HashSet<Guid> usedIds,
        IMapper mapper,
        ContentLookups lookups,
        IFileStorageService fileStorage,
        IReadOnlySet<Guid> likedArticleIds,
        IReadOnlySet<Guid> bookmarkedArticleIds,
        CancellationToken cancellationToken
    )
    {
        if (promoted.Count > 0)
        {
            return new ArticlePromotionSpotDto(
                SpotPriority: spotPriority,
                Articles: await promoted.ToPublicArticleSummaryDtosAsync(
                    lookups,
                    fileStorage,
                    likedArticleIds,
                    bookmarkedArticleIds,
                    cancellationToken
                )
            );
        }

        var fallback = new List<PublicArticleSummaryDto>();

        if (gossipQueue.TryDequeue(out ArticleEntity? gossip))
        {
            usedIds.Add(gossip.Id);
            fallback.Add(
                await gossip.ToPublicArticleSummaryDtoAsync(
                    lookups,
                    fileStorage,
                    likedArticleIds,
                    bookmarkedArticleIds,
                    cancellationToken
                )
            );
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
    /// <param name="lookups">The categories the cards name, resolved for the whole feed.</param>
    /// <param name="fileStorage">Core's storage contract.</param>
    /// <param name="likedArticleIds">Ids the current user has liked.</param>
    /// <param name="bookmarkedArticleIds">Ids the current user has bookmarked.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>
    /// A <see cref="ArticlePromotionSpot3Dto" /> with two named slots (<c>"a"</c> and <c>"b"</c>),
    /// each containing at least one article.
    /// </returns>
    private static async Task<ArticlePromotionSpot3Dto> BuildSpot3Async(
        IReadOnlyList<ArticleEntity> promoted,
        Queue<ArticleEntity> gossipQueue,
        HashSet<Guid> usedIds,
        IMapper mapper,
        ContentLookups lookups,
        IFileStorageService fileStorage,
        IReadOnlySet<Guid> likedArticleIds,
        IReadOnlySet<Guid> bookmarkedArticleIds,
        CancellationToken cancellationToken
    )
    {
        var columnA = new List<PublicArticleSummaryDto>();
        var columnB = new List<PublicArticleSummaryDto>();

        for (int i = 0; i < promoted.Count; i++)
        {
            PublicArticleSummaryDto dto = await promoted[i]
                .ToPublicArticleSummaryDtoAsync(
                    lookups,
                    fileStorage,
                    likedArticleIds,
                    bookmarkedArticleIds,
                    cancellationToken
                );
            (i % 2 == 0 ? columnA : columnB).Add(dto);
        }

        if (columnA.Count == 0 && gossipQueue.TryDequeue(out ArticleEntity? gossipA))
        {
            usedIds.Add(gossipA.Id);
            columnA.Add(
                await gossipA.ToPublicArticleSummaryDtoAsync(
                    lookups,
                    fileStorage,
                    likedArticleIds,
                    bookmarkedArticleIds,
                    cancellationToken
                )
            );
        }

        if (columnB.Count == 0 && gossipQueue.TryDequeue(out ArticleEntity? gossipB))
        {
            usedIds.Add(gossipB.Id);
            columnB.Add(
                await gossipB.ToPublicArticleSummaryDtoAsync(
                    lookups,
                    fileStorage,
                    likedArticleIds,
                    bookmarkedArticleIds,
                    cancellationToken
                )
            );
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
    /// maps them to <see cref="PublicArticleSummaryDto" /> for the horizontal gossip strip.
    /// </summary>
    /// <param name="gossipQueue">Remaining gossip articles not yet consumed by spot fallbacks.</param>
    /// <param name="stripSize">Maximum number of articles to include in the strip.</param>
    /// <param name="mapper">Mapster mapper for entity-to-DTO transformations.</param>
    /// <param name="lookups">The categories the cards name, resolved for the whole feed.</param>
    /// <param name="fileStorage">Core's storage contract.</param>
    /// <param name="likedArticleIds">Ids the current user has liked.</param>
    /// <param name="bookmarkedArticleIds">Ids the current user has bookmarked.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>
    /// An ordered list of up to <paramref name="stripSize" /> gossip article summaries.
    /// May be shorter if the queue is exhausted.
    /// </returns>
    private static async Task<IReadOnlyList<PublicArticleSummaryDto>> BuildGossipStripAsync(
        Queue<ArticleEntity> gossipQueue,
        int stripSize,
        IMapper mapper,
        ContentLookups lookups,
        IFileStorageService fileStorage,
        IReadOnlySet<Guid> likedArticleIds,
        IReadOnlySet<Guid> bookmarkedArticleIds,
        CancellationToken cancellationToken
    )
    {
        var strip = new List<PublicArticleSummaryDto>();

        while (strip.Count < stripSize && gossipQueue.TryDequeue(out ArticleEntity? article))
        {
            strip.Add(
                await article.ToPublicArticleSummaryDtoAsync(
                    lookups,
                    fileStorage,
                    likedArticleIds,
                    bookmarkedArticleIds,
                    cancellationToken
                )
            );
        }

        return strip;
    }
}
