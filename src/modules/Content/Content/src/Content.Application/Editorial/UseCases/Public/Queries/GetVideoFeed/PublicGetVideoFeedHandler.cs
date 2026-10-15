using _116.BuildingBlocks.Application.CQRS;
using _116.Content.Application.Catalog.Services;
using _116.Content.Application.Editorial.Services;
using _116.Content.Application.Editorial.UseCases.Public.Queries.GetVideoFeed.Contracts;
using _116.Content.Application.Shared.DTOs;
using _116.Content.Domain.Entities;

namespace _116.Content.Application.Editorial.UseCases.Public.Queries.GetVideoFeed;

/// <summary>
/// Handles the <see cref="PublicGetVideoFeedQuery" /> to serve the video feed: one entry per
/// pinned video category, each with its latest published videos.
/// </summary>
/// <param name="feedService">Service resolving the pinned entries.</param>
/// <param name="categoryDtoService">Service assembling the category DTOs.</param>
/// <param name="videoDtoService">Service assembling the public video summaries.</param>
public class PublicGetVideoFeedHandler(
    IPublicVideoFeedService feedService,
    ICategoryDtoService categoryDtoService,
    IVideoDtoService videoDtoService
) : IQueryHandler<PublicGetVideoFeedQuery, PublicGetVideoFeedResult>
{
    /// <inheritdoc />
    public async Task<PublicGetVideoFeedResult> Handle(
        PublicGetVideoFeedQuery query,
        CancellationToken cancellationToken
    )
    {
        IReadOnlyList<VideoFeed> feeds = await feedService.GetPinnedFeedsAsync(cancellationToken);

        // Empty entries are omitted so the UI never renders a blank block.
        IReadOnlyList<VideoFeed> filled = [.. feeds.Where(feed => feed.Videos.Count > 0)];

        if (filled.Count == 0)
        {
            return new PublicGetVideoFeedResult(Items: []);
        }

        // One batch each for the categories and for every entry's videos.
        IReadOnlyList<CategoryDto> categoryDtos = await categoryDtoService.CreateManyAsync(
            [.. filled.Select(feed => feed.Category)],
            cancellationToken
        );
        PublicVideoSummaryContext context = await videoDtoService.ResolvePublicContextAsync(
            [.. filled.SelectMany(feed => feed.Videos)],
            cancellationToken
        );

        List<VideoFeedDto> result =
        [
            .. filled.Select(
                (feed, index) =>
                    new VideoFeedDto(
                        Category: categoryDtos[index],
                        Videos: videoDtoService.CreatePublicMany(feed.Videos, context)
                    )
            ),
        ];

        return new PublicGetVideoFeedResult(Items: result);
    }
}
