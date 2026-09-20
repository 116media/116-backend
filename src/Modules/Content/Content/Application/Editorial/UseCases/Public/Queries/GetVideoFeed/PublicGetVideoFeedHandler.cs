using _116.Content.Application.Catalog.Factories;
using _116.Content.Application.Editorial.Constants;
using _116.Content.Application.Shared.DTOs;
using _116.Content.Application.Shared.Mappers;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Core.Contracts.Application.DTOs;
using _116.Core.Contracts.Application.Services;
using _116.Shared.Contracts.Application.CQRS;
using MapsterMapper;

namespace _116.Content.Application.Editorial.UseCases.Public.Queries.GetVideoFeed;

/// <summary>
/// Handles the <see cref="PublicGetVideoFeedQuery" /> to build the public video feed:
/// one section per pinned video category, each carrying its latest published videos.
/// Resolves all poster and thumbnail URLs in a single batched file lookup to avoid an N+1.
/// </summary>
/// <param name="categoryRepository">Repository for category data access operations.</param>
/// <param name="contentTypeRepository">Repository resolving the pinned categories' content types.</param>
/// <param name="categoryDtoFactory">Builds category projections with their lookups resolved.</param>
/// <param name="videoRepository">Repository for video data access operations.</param>
/// <param name="fileStorage">Core's storage contract.</param>
/// <param name="mapper">Mapster mapper for entity-to-DTO transformations.</param>
public class PublicGetVideoFeedHandler(
    ICategoryRepository categoryRepository,
    IContentTypeRepository contentTypeRepository,
    ICategoryDtoFactory categoryDtoFactory,
    IContentLookupFactory contentLookupFactory,
    IVideoRepository videoRepository,
    IFileStorageService fileStorage,
    IMapper mapper
) : IQueryHandler<PublicGetVideoFeedQuery, PublicGetVideoFeedResult>
{
    /// <inheritdoc />
    public async Task<PublicGetVideoFeedResult> Handle(
        PublicGetVideoFeedQuery query,
        CancellationToken cancellationToken
    )
    {
        // The pinned set is capped per content type, so resolving their types in one lookup and
        // filtering to Video in memory is trivial.
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

        // One query for every video thumbnail; the category posters come with the lookups below.
        var thumbnailIds = videosByCategory
            .Values.SelectMany(videos => videos)
            .Where(v => v.ThumbnailFileId.HasValue)
            .Select(v => v.ThumbnailFileId!.Value)
            .Distinct()
            .ToList();

        IReadOnlyDictionary<Guid, FileReferenceDto> files = await fileStorage.ResolveManyAsync(
            fileIds: thumbnailIds,
            cancellationToken: cancellationToken
        );

        // The pinned categories' posters, content types and pricing tiers, in one batch.
        CategoryLookups categoryLookups = await categoryDtoFactory.ResolveLookupsAsync(
            videoCategories,
            cancellationToken
        );

        // One batch for the categories, customers and promotion levels the cards name.
        ContentLookups videoLookups = await contentLookupFactory.ResolveForVideosAsync(
            [.. videosByCategory.Values.SelectMany(videos => videos)],
            cancellationToken
        );

        // One query for the published-lyrics fact across every section's videos.
        IReadOnlySet<Guid> videosWithLyrics = await videoRepository.GetIdsWithPublishedLyricsAsync(
            videoIds: videosByCategory.Values.SelectMany(videos => videos).Select(v => v.Id).ToList(),
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

            CategoryDto categoryDto = category.ToCategoryDto(mapper, categoryLookups);
            IReadOnlyList<PublicVideoSummaryDto> videoDtos = videos
                .Select(v => v.ToPublicVideoSummaryDto(videoLookups, files, videosWithLyrics.Contains(v.Id)))
                .ToList();

            sections.Add(new VideoFeedSectionDto(Category: categoryDto, Videos: videoDtos));
        }

        return new PublicGetVideoFeedResult(Sections: sections);
    }
}
