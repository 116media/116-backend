using _116.Content.Application.Editorial.Constants;
using _116.Content.Application.Shared.DTOs;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Editorial.UseCases.Public.Queries.GetVideoPromotionFeed;

/// <summary>
/// Query for retrieving the homepage video promotion feed, grouped by spot priority.
/// </summary>
/// <param name="StripSize">
/// Number of free videos to include in the free-video strip.
/// Defaults to <see cref="EditorialFeedConstants.DefaultStripSize" />.
/// </param>
public record PublicGetVideoPromotionFeedQuery(int StripSize = EditorialFeedConstants.DefaultStripSize)
    : IQuery<PublicGetVideoPromotionFeedResult>;

/// <summary>
/// A column slot within spot 3, identified by its position label.
/// </summary>
/// <param name="Position">The column label: <c>"a"</c> or <c>"b"</c>.</param>
/// <param name="Videos">The videos assigned to this column.</param>
public record VideoPromotionSlotDto(string Position, IReadOnlyList<VideoSummaryDto> Videos);

/// <summary>
/// A simple promotion spot (1 or 2) with its list of videos.
/// </summary>
/// <param name="SpotPriority">The spot priority number (1 or 2).</param>
/// <param name="Videos">The videos displayed in this spot.</param>
public record VideoPromotionSpotDto(int SpotPriority, IReadOnlyList<VideoSummaryDto> Videos);

/// <summary>
/// The special spot 3 layout, which distributes videos across two columns.
/// </summary>
/// <param name="SpotPriority">Always 3.</param>
/// <param name="Slots">Column slots: one for <c>"a"</c>, one for <c>"b"</c>.</param>
public record VideoPromotionSpot3Dto(int SpotPriority, IReadOnlyList<VideoPromotionSlotDto> Slots);

/// <summary>
/// Result of the <see cref="PublicGetVideoPromotionFeedQuery" />.
/// </summary>
/// <param name="Spot1">Promoted videos for spot 1.</param>
/// <param name="Spot2">Promoted videos for spot 2.</param>
/// <param name="Spot3">Promoted videos for spot 3, split into two columns.</param>
/// <param name="FreeVideoStrip">Randomly selected free videos shown in the horizontal strip below the grid.</param>
public record PublicGetVideoPromotionFeedResult(
    VideoPromotionSpotDto Spot1,
    VideoPromotionSpotDto Spot2,
    VideoPromotionSpot3Dto Spot3,
    IReadOnlyList<VideoSummaryDto> FreeVideoStrip
);
