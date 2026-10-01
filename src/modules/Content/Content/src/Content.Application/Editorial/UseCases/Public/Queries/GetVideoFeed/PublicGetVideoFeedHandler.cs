using _116.BuildingBlocks.Application.CQRS;
using _116.Content.Application.Catalog.Services;
using _116.Content.Application.Editorial.Services;
using _116.Content.Application.Editorial.UseCases.Public.Queries.GetVideoFeed.Contracts;
using _116.Content.Application.Shared.DTOs;
using _116.Content.Domain.Entities;

namespace _116.Content.Application.Editorial.UseCases.Public.Queries.GetVideoFeed;

/// <summary>
/// Handles the <see cref="PublicGetVideoFeedQuery" /> to serve the video feed: one section per
/// pinned video category, each with its latest published videos.
/// </summary>
/// <param name="feedService">Service resolving the pinned sections.</param>
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
        IReadOnlyList<VideoFeedSection> sections = await feedService.GetPinnedSectionsAsync(cancellationToken);

        // Empty sections are omitted so the UI never renders a blank block.
        IReadOnlyList<VideoFeedSection> filled = [.. sections.Where(section => section.Videos.Count > 0)];

        if (filled.Count == 0)
        {
            return new PublicGetVideoFeedResult(Sections: []);
        }

        // One batch each for the categories and for every section's videos.
        IReadOnlyList<CategoryDto> categoryDtos = await categoryDtoService.CreateManyAsync(
            [.. filled.Select(section => section.Category)],
            cancellationToken
        );
        PublicVideoSummaryContext context = await videoDtoService.ResolvePublicContextAsync(
            [.. filled.SelectMany(section => section.Videos)],
            cancellationToken
        );

        List<VideoFeedSectionDto> result =
        [
            .. filled.Select(
                (section, index) =>
                    new VideoFeedSectionDto(
                        Category: categoryDtos[index],
                        Videos: videoDtoService.CreatePublicMany(section.Videos, context)
                    )
            ),
        ];

        return new PublicGetVideoFeedResult(Sections: result);
    }
}
