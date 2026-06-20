using _116.Content.Application.Editorial.Constants;
using _116.Content.Application.Shared.DTOs;
using _116.Content.Application.Shared.Mappers;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Core.Application.Shared.Repositories;
using _116.Core.Domain.Entities;
using _116.Shared.Contracts.Application.CQRS;
using MapsterMapper;

namespace _116.Content.Application.Editorial.UseCases.Public.Queries.GetVideoFeed;

/// <summary>
/// Handles the <see cref="PublicGetVideoFeedQuery" /> to build the public video feed:
/// one section per pinned video category, each carrying its latest published videos.
/// Resolves all poster and thumbnail URLs in a single batched file lookup to avoid an N+1.
/// </summary>
/// <param name="categoryRepository">Repository for category data access operations.</param>
/// <param name="videoRepository">Repository for video data access operations.</param>
/// <param name="fileRepository">Repository for resolving file URLs.</param>
/// <param name="mapper">Mapster mapper for entity-to-DTO transformations.</param>
public class PublicGetVideoFeedHandler(
    ICategoryRepository categoryRepository,
    IVideoRepository videoRepository,
    IFileRepository fileRepository,
    IMapper mapper
) : IQueryHandler<PublicGetVideoFeedQuery, PublicGetVideoFeedResult>
{
    /// <inheritdoc />
    public async Task<PublicGetVideoFeedResult> Handle(
        PublicGetVideoFeedQuery query,
        CancellationToken cancellationToken
    )
    {
        // Pinned categories (ContentType is eager-loaded by the repo). The pinned set is capped
        // per content type, so filtering to Video in memory is trivial and avoids a separate query.
        IReadOnlyList<CategoryEntity> pinned = await categoryRepository.GetPinnedToFeedCategoriesAsync(
            cancellationToken: cancellationToken
        );

        List<CategoryEntity> videoCategories = pinned
            .Where(c => c.ContentType.Name == nameof(EnumCoreContentType.Video))
            .ToList();

        if (videoCategories.Count == 0)
        {
            return new PublicGetVideoFeedResult(Sections: []);
        }

        // Latest published videos per category. Bounded by the cap, so a small fixed number of
        // indexed lookups rather than an unbounded N+1.
        var videosByCategory = new Dictionary<Guid, IReadOnlyList<VideoEntity>>(videoCategories.Count);

        foreach (CategoryEntity category in videoCategories)
        {
            videosByCategory[category.Id] = await videoRepository.GetLatestPublishedByCategoryAsync(
                categoryId: category.Id,
                limit: EditorialFeedConstants.MaxVideosPerFeedSection,
                cancellationToken: cancellationToken
            );
        }

        // One query for ALL file URLs (category posters + video thumbnails).
        var fileIds = videoCategories
            .Where(c => c.PosterFileId.HasValue)
            .Select(c => c.PosterFileId!.Value)
            .Concat(
                videosByCategory
                    .Values.SelectMany(videos => videos)
                    .Where(v => v.ThumbnailFileId.HasValue)
                    .Select(v => v.ThumbnailFileId!.Value)
            )
            .Distinct()
            .ToList();

        IReadOnlyDictionary<Guid, FileEntity> files = await fileRepository.GetByIdsAsync(
            fileIds: fileIds,
            cancellationToken: cancellationToken
        );

        // Pure in-memory assembly — the map-from-dictionary overloads do no IO.
        var sections = new List<VideoFeedSectionDto>(videoCategories.Count);

        foreach (CategoryEntity category in videoCategories)
        {
            IReadOnlyList<VideoEntity> videos = videosByCategory[category.Id];

            // Omit empty sections so the UI never renders a blank block.
            if (videos.Count == 0)
            {
                continue;
            }

            CategoryDto categoryDto = category.ToCategoryDto(mapper, files);
            IReadOnlyList<VideoSummaryDto> videoDtos = videos.Select(v => v.ToVideoSummaryDto(mapper, files)).ToList();

            sections.Add(new VideoFeedSectionDto(Category: categoryDto, Videos: videoDtos));
        }

        return new PublicGetVideoFeedResult(Sections: sections);
    }
}
