using _116.Content.Application.Shared.DTOs;
using _116.Content.Domain.Entities;
using _116.Core.Contracts.Application.DTOs;
using Mapster;
using MapsterMapper;

namespace _116.Content.Application.Shared.Mappers;

/// <summary>
/// Mapster configuration for Video entity mappings.
/// </summary>
public static class VideoMapper
{
    /// <summary>
    /// Registers Video entity mappings into the provided TypeAdapterConfig.
    /// </summary>
    /// <remarks>
    /// <c>VideoEntity → VideoSummaryDto</c> and <c>VideoEntity → VideoDetailDto</c> are
    /// intentionally NOT registered here. Registering them causes Mapster to auto-flatten the
    /// <c>PromotionLevel</c> navigation property (which shares field names like <c>Id</c>,
    /// <c>CreatedAt</c>, <c>CreatedBy</c> with the destination DTO base) and then NPEs at runtime
    /// when <c>PromotionLevel</c> is null. Those two mappings are handled as plain C# in the
    /// extension methods below.
    /// </remarks>
    /// <param name="config">
    /// The TypeAdapterConfig to register mappings into.
    /// </param>
    public static void Register(TypeAdapterConfig config)
    {
        config
            .NewConfig<TagEntity, TagDto>()
            .Map(dest => dest.Id, src => src.Id)
            .Map(dest => dest.Name, src => src.Name)
            .Map(dest => dest.Slug, src => src.Slug);
    }

    /// <summary>
    /// Maps a <see cref="VideoEntity" /> to a <see cref="VideoDetailDto" /> from an already
    /// resolved thumbnail URL. Performs no IO.
    /// </summary>
    public static VideoDetailDto ToVideoDetailDto(
        this VideoEntity entity,
        IMapper mapper,
        ContentLookups lookups,
        string? thumbnailUrl,
        bool hasLyrics,
        short? ratedStars = null
    )
    {
        return new VideoDetailDto(
            entity.Id,
            entity.CategoryId,
            lookups.CategoryName(entity.CategoryId),
            entity.Title,
            entity.Slug,
            entity.Description,
            thumbnailUrl,
            entity.AuthorId.ToString(),
            entity.Status,
            entity.RejectionReason,
            entity.YoutubeVideoUrl,
            entity.SocialBoost,
            entity.IsPromoted,
            entity.PromotedUntil,
            entity.PromotionLevelId,
            lookups.PromotionLevelName(entity.PromotionLevelId),
            hasLyrics,
            entity.ShootingScheduledAt,
            entity.PublishedAt,
            entity.MetaTitle,
            entity.MetaDescription,
            entity.TagDtos(mapper, lookups),
            entity.ShareCount,
            entity.RatingAverage,
            entity.RatingCount,
            entity.CustomerId,
            lookups.CustomerName(entity.CustomerId),
            entity.OrderItemId,
            IsRated: ratedStars.HasValue,
            RatedStars: ratedStars
        )
        {
            CreatedAt = entity.CreatedAt,
            CreatedBy = entity.CreatedBy,
            UpdatedAt = entity.UpdatedAt,
            UpdatedBy = entity.UpdatedBy,
        };
    }

    /// <summary>
    /// Maps a list of videos to their public card projection using a pre-fetched file map.
    /// Performs no IO — batch mappings resolve files up front.
    /// </summary>
    public static IReadOnlyList<PublicVideoSummaryDto> ToPublicVideoSummaryDtos(
        this IReadOnlyList<VideoEntity> entities,
        ContentLookups lookups,
        IReadOnlyDictionary<Guid, FileReferenceDto> files,
        IReadOnlySet<Guid> videosWithLyrics
    )
    {
        return entities
            .Select(entity => entity.ToPublicVideoSummaryDto(lookups, files, videosWithLyrics.Contains(entity.Id)))
            .ToList();
    }

    /// <summary>
    /// Maps a <see cref="VideoEntity" /> to its public card projection from a pre-fetched
    /// file map. Performs no IO — batch mappings resolve files up front.
    /// </summary>
    public static PublicVideoSummaryDto ToPublicVideoSummaryDto(
        this VideoEntity entity,
        ContentLookups lookups,
        IReadOnlyDictionary<Guid, FileReferenceDto> files,
        bool hasLyrics
    )
    {
        string? thumbnailUrl =
            entity.ThumbnailFileId is { } thumbnailId && files.TryGetValue(thumbnailId, out FileReferenceDto? thumbnail)
                ? thumbnail.StorageUrl
                : null;

        return entity.ToPublicVideoSummaryDto(lookups: lookups, thumbnailUrl: thumbnailUrl, hasLyrics: hasLyrics);
    }

    /// <summary>
    /// Maps a <see cref="VideoEntity" /> to its public card projection from an already
    /// resolved thumbnail URL. Performs no IO — batch mappings resolve files up front.
    /// </summary>
    public static PublicVideoSummaryDto ToPublicVideoSummaryDto(
        this VideoEntity entity,
        ContentLookups lookups,
        string? thumbnailUrl,
        bool hasLyrics
    )
    {
        return new PublicVideoSummaryDto(
            entity.Id,
            entity.CategoryId,
            lookups.CategoryName(entity.CategoryId),
            entity.Title,
            entity.Slug,
            thumbnailUrl,
            entity.YoutubeVideoUrl,
            entity.IsPromoted,
            hasLyrics,
            entity.PublishedAt,
            entity.ShareCount,
            entity.RatingAverage,
            entity.RatingCount
        );
    }

    /// <summary>
    /// Maps a <see cref="VideoEntity" /> to its public detail projection from an already resolved
    /// thumbnail URL, stamping the current user's rating. Performs no IO.
    /// </summary>
    public static PublicVideoDetailDto ToPublicVideoDetailDto(
        this VideoEntity entity,
        IMapper mapper,
        ContentLookups lookups,
        string? thumbnailUrl,
        bool hasLyrics,
        short? ratedStars = null
    )
    {
        return new PublicVideoDetailDto(
            entity.Id,
            entity.CategoryId,
            lookups.CategoryName(entity.CategoryId),
            entity.Title,
            entity.Slug,
            entity.Description,
            thumbnailUrl,
            entity.YoutubeVideoUrl,
            entity.IsPromoted,
            hasLyrics,
            entity.PublishedAt,
            entity.MetaTitle,
            entity.MetaDescription,
            entity.TagDtos(mapper, lookups),
            entity.ShareCount,
            entity.RatingAverage,
            entity.RatingCount,
            IsRated: ratedStars.HasValue,
            RatedStars: ratedStars
        );
    }

    /// <summary>
    /// Maps a <see cref="VideoEntity" /> to a <see cref="VideoSummaryDto" />, resolving the
    /// thumbnail URL from a pre-fetched file map. Performs no IO — intended for batch mapping
    /// (e.g. the content feed) where files are loaded once up front via
    /// <c>IFileRepository.GetByIdsAsync</c>.
    /// </summary>
    public static VideoSummaryDto ToVideoSummaryDto(
        this VideoEntity entity,
        IMapper mapper,
        ContentLookups lookups,
        IReadOnlyDictionary<Guid, FileReferenceDto> files,
        bool hasLyrics
    )
    {
        string? thumbnailUrl =
            entity.ThumbnailFileId is { } thumbnailId && files.TryGetValue(thumbnailId, out FileReferenceDto? thumbnail)
                ? thumbnail.StorageUrl
                : null;

        return new VideoSummaryDto(
            entity.Id,
            entity.CategoryId,
            lookups.CategoryName(entity.CategoryId),
            entity.Title,
            entity.Slug,
            thumbnailUrl,
            entity.AuthorId.ToString(),
            entity.Status,
            entity.YoutubeVideoUrl,
            entity.IsPromoted,
            hasLyrics,
            entity.PublishedAt,
            entity.ShootingScheduledAt,
            entity.ShareCount,
            entity.RatingAverage,
            entity.RatingCount
        )
        {
            CreatedAt = entity.CreatedAt,
            CreatedBy = entity.CreatedBy,
            UpdatedAt = entity.UpdatedAt,
            UpdatedBy = entity.UpdatedBy,
        };
    }

    /// <summary>
    /// Projects a video's tag junction rows through the resolved tag map, dropping any tag row
    /// that no longer exists.
    /// </summary>
    /// <param name="entity">The video whose tags to project.</param>
    /// <param name="mapper">Injected IMapper instance.</param>
    /// <param name="lookups">The resolved rows, including the tags.</param>
    /// <returns>The tag projections.</returns>
    private static IReadOnlyList<TagDto> TagDtos(this VideoEntity entity, IMapper mapper, ContentLookups lookups)
    {
        return
        [
            .. entity
                .Tags.Select(videoTag => lookups.Tags.GetValueOrDefault(videoTag.TagId))
                .OfType<TagEntity>()
                .Select(mapper.Map<TagDto>),
        ];
    }
}
