using _116.Content.Application.Editorial.Constants;
using _116.Content.Application.Editorial.UseCases.Public.Queries.GetVideoFeed.Contracts;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;

namespace _116.Content.Application.Editorial.UseCases.Public.Queries.GetVideoFeed;

/// <summary>
/// Resolves the entries of the video feed for the public video-feed query.
/// </summary>
/// <param name="categoryRepository">Repository resolving the pinned categories.</param>
/// <param name="contentTypeRepository">Repository resolving their content types.</param>
/// <param name="videoRepository">Repository loading each entry's latest videos.</param>
public class PublicVideoFeedService(
    ICategoryRepository categoryRepository,
    IContentTypeRepository contentTypeRepository,
    IVideoRepository videoRepository
) : IPublicVideoFeedService
{
    /// <inheritdoc />
    public async Task<IReadOnlyList<VideoFeed>> GetPinnedFeedsAsync(CancellationToken cancellationToken)
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
        var feeds = new List<VideoFeed>(videoCategories.Count);

        foreach (CategoryEntity category in videoCategories)
        {
            IReadOnlyList<VideoEntity> videos = await videoRepository.GetLatestPublishedByCategoryAsync(
                categoryId: category.Id,
                limit: EditorialFeedConstants.MaxVideosPerFeed,
                cancellationToken: cancellationToken
            );
            feeds.Add(new VideoFeed(Category: category, Videos: videos));
        }

        return feeds;
    }
}
