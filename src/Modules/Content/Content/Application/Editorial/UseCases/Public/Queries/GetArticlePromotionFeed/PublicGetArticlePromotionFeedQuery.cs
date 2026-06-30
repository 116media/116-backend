using _116.Content.Application.Editorial.Constants;
using _116.Content.Application.Shared.DTOs;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Editorial.UseCases.Public.Queries.GetArticlePromotionFeed;

/// <summary>
/// Query for retrieving the homepage article promotion feed, grouped by spot priority.
/// </summary>
/// <param name="StripSize">
/// Number of gossip articles to include in the gossip strip.
/// Defaults to <see cref="EditorialFeedConstants.DefaultStripSize" />.
/// </param>
/// <param name="CurrentUserId">
/// The authenticated caller's id, or null for an anonymous request. When null, the per-user
/// interaction flags on the returned summaries resolve to false.
/// </param>
public record PublicGetArticlePromotionFeedQuery(
    int StripSize = EditorialFeedConstants.DefaultStripSize,
    Guid? CurrentUserId = null
) : IQuery<PublicGetArticlePromotionFeedResult>;

/// <summary>
/// A single promoted article entry for the feed.
/// </summary>
/// <param name="Article">The article summary DTO.</param>
public record ArticlePromotionEntryDto(ArticleSummaryDto Article);

/// <summary>
/// A column slot within spot 3, identified by its position label.
/// </summary>
/// <param name="Position">The column label: <c>"a"</c> or <c>"b"</c>.</param>
/// <param name="Articles">The articles assigned to this column.</param>
public record ArticlePromotionSlotDto(string Position, IReadOnlyList<ArticleSummaryDto> Articles);

/// <summary>
/// A simple promotion spot (1 or 2) with its list of articles.
/// </summary>
/// <param name="SpotPriority">The spot priority number (1 or 2).</param>
/// <param name="Articles">The articles displayed in this spot.</param>
public record ArticlePromotionSpotDto(int SpotPriority, IReadOnlyList<ArticleSummaryDto> Articles);

/// <summary>
/// The special spot 3 layout, which distributes articles across two columns.
/// </summary>
/// <param name="SpotPriority">Always 3.</param>
/// <param name="Slots">Column slots: one for <c>"a"</c>, one for <c>"b"</c>.</param>
public record ArticlePromotionSpot3Dto(int SpotPriority, IReadOnlyList<ArticlePromotionSlotDto> Slots);

/// <summary>
/// Result of the <see cref="PublicGetArticlePromotionFeedQuery" />.
/// </summary>
/// <param name="Spot1">Promoted articles for spot 1.</param>
/// <param name="Spot2">Promoted articles for spot 2.</param>
/// <param name="Spot3">Promoted articles for spot 3, split into two columns.</param>
/// <param name="GossipStrip">Gossip articles shown in the horizontal strip below the grid.</param>
public record PublicGetArticlePromotionFeedResult(
    ArticlePromotionSpotDto Spot1,
    ArticlePromotionSpotDto Spot2,
    ArticlePromotionSpot3Dto Spot3,
    IReadOnlyList<ArticleSummaryDto> GossipStrip
);
