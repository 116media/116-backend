using _116.Content.Application.Editorial.Constants;
using _116.Content.Application.Editorial.UseCases.Public.Queries.GetVideoFeed.Contracts;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;

namespace _116.Content.Application.Editorial.UseCases.Public.Queries.GetVideoFeed;

/// <summary>
/// Resolves the video feed's sections for the public video-feed query.
/// </summary>
/// <param name="categoryRepository">Repository resolving the pinned categories.</param>
/// <param name="contentTypeRepository">Repository resolving their content types.</param>
/// <param name="videoRepository">Repository loading each section's latest videos.</param>
public class PublicVideoFeedService(
    ICategoryRepository categoryRepository,
    IContentTypeRepository contentTypeRepository,
    IVideoRepository videoRepository
) : IPublicVideoFeedService
{
    /// <inheritdoc />
    public async Task<IReadOnlyList<VideoFeedSection>> GetPinnedSectionsAsync(CancellationToken cancellationToken)
    {
        // The pinned set is capped per content type, so one lookup and an in-memory filter suffice.
        IReadOnlyList<CategoryEntity> pinned = await categoryRepository.GetPinnedToFeedCategoriesAsync(
            cancellationToken: cancellationToken
        );
        IReadOnlyDictionary<Guid, ContentTypeEntity> contentTypes = await contentTypeRepository.GetByIdsAsync(
            ids: [.. pinned.Select(category => category.ContentTypeId).Distinct()],
            cancellationToken: cancellationToken
        );

        List<CategoryEntity> videoCategories =
        [
            .. pinned.Where(category =>
                contentTypes.TryGetValue(category.ContentTypeId, out ContentTypeEntity? contentType)
                && contentType.Name == nameof(EnumCoreContentType.Video)
            ),
        ];

        // Bounded by the pin cap, so a small fixed number of indexed lookups rather than an unbounded N+1.
        var sections = new List<VideoFeedSection>(videoCategories.Count);

        foreach (CategoryEntity category in videoCategories)
        {
            IReadOnlyList<VideoEntity> videos = await videoRepository.GetLatestPublishedByCategoryAsync(
                categoryId: category.Id,
                limit: EditorialFeedConstants.MaxVideosPerFeedSection,
                cancellationToken: cancellationToken
            );
            sections.Add(new VideoFeedSection(Category: category, Videos: videos));
        }

        return sections;
    }
}
