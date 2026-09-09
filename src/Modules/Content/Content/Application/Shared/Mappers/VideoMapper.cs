using _116.Content.Application.Shared.DTOs;
using _116.Content.Domain.Entities;
using _116.Core.Contracts.Application.DTOs;
using _116.Core.Contracts.Application.Services;
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
            .NewConfig<VideoTagEntity, TagDto>()
            .Map(dest => dest.Id, src => src.Tag.Id)
            .Map(dest => dest.Name, src => src.Tag.Name)
            .Map(dest => dest.Slug, src => src.Tag.Slug);
    }

    /// <summary>
    /// Maps a <see cref="VideoEntity" /> to a <see cref="VideoDetailDto" />,
    /// resolving the thumbnail URL from the associated FileEntity.
    /// </summary>
    public static async Task<VideoDetailDto> ToVideoDetailDtoAsync(
        this VideoEntity entity,
        IMapper mapper,
        IFileStorageService fileStorage,
        CancellationToken ct = default,
        short? ratedStars = null
    )
    {
        string? thumbnailUrl = await ResolveThumbnailUrlAsync(entity, fileStorage, ct);

        return new VideoDetailDto(
            entity.Id,
            entity.CategoryId,
            entity.Category != null ? entity.Category.Name : string.Empty,
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
            entity.PromotionLevel?.Name,
            entity.HasLyrics,
            entity.ShootingScheduledAt,
            entity.PublishedAt,
            entity.MetaTitle,
            entity.MetaDescription,
            mapper.Map<IReadOnlyList<TagDto>>(entity.Tags),
            entity.ShareCount,
            entity.RatingAverage,
            entity.RatingCount,
            entity.CustomerId,
            entity.Customer != null ? entity.Customer.FullName : null,
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
    /// Maps a list of videos to their public card projection, thumbnail URLs resolved in one
    /// batch.
    /// </summary>
    public static async Task<IReadOnlyList<PublicVideoSummaryDto>> ToPublicVideoSummaryDtosAsync(
        this IReadOnlyList<VideoEntity> entities,
        IFileStorageService fileStorage,
        CancellationToken ct = default
    )
    {
        IReadOnlyDictionary<Guid, FileReferenceDto> files = await fileStorage.ResolveManyAsync(
            entities.Where(e => e.ThumbnailFileId.HasValue).Select(e => e.ThumbnailFileId!.Value).Distinct().ToList(),
            ct
        );

        return entities.ToPublicVideoSummaryDtos(files);
    }

    /// <summary>
    /// Maps a list of videos to their public card projection using a pre-fetched file map.
    /// Performs no IO — batch mappings resolve files up front.
    /// </summary>
    public static IReadOnlyList<PublicVideoSummaryDto> ToPublicVideoSummaryDtos(
        this IReadOnlyList<VideoEntity> entities,
        IReadOnlyDictionary<Guid, FileReferenceDto> files
    )
    {
        return entities.Select(entity => entity.ToPublicVideoSummaryDto(files)).ToList();
    }

    /// <summary>
    /// Maps a <see cref="VideoEntity" /> to its public card projection from a pre-fetched
    /// file map. Performs no IO — batch mappings resolve files up front.
    /// </summary>
    public static PublicVideoSummaryDto ToPublicVideoSummaryDto(
        this VideoEntity entity,
        IReadOnlyDictionary<Guid, FileReferenceDto> files
    )
    {
        string? thumbnailUrl =
            entity.ThumbnailFileId is { } thumbnailId && files.TryGetValue(thumbnailId, out FileReferenceDto? thumbnail)
                ? thumbnail.StorageUrl
                : null;

        return entity.ToPublicVideoSummaryDto(thumbnailUrl: thumbnailUrl);
    }

    /// <summary>
    /// Maps a <see cref="VideoEntity" /> to its public card projection, resolving the
    /// thumbnail URL from the associated FileEntity.
    /// </summary>
    public static async Task<PublicVideoSummaryDto> ToPublicVideoSummaryDtoAsync(
        this VideoEntity entity,
        IFileStorageService fileStorage,
        CancellationToken ct = default
    )
    {
        string? thumbnailUrl = await ResolveThumbnailUrlAsync(entity, fileStorage, ct);
        return entity.ToPublicVideoSummaryDto(thumbnailUrl: thumbnailUrl);
    }

    /// <summary>
    /// Maps a <see cref="VideoEntity" /> to its public card projection from an already
    /// resolved thumbnail URL. Performs no IO — batch mappings resolve files up front.
    /// </summary>
    public static PublicVideoSummaryDto ToPublicVideoSummaryDto(this VideoEntity entity, string? thumbnailUrl)
    {
        return new PublicVideoSummaryDto(
            entity.Id,
            entity.CategoryId,
            entity.Category != null ? entity.Category.Name : string.Empty,
            entity.Title,
            entity.Slug,
            thumbnailUrl,
            entity.YoutubeVideoUrl,
            entity.IsPromoted,
            entity.HasLyrics,
            entity.PublishedAt,
            entity.ShareCount,
            entity.RatingAverage,
            entity.RatingCount
        );
    }

    /// <summary>
    /// Maps a <see cref="VideoEntity" /> to its public detail projection, resolving the
    /// thumbnail URL and stamping the current user's rating.
    /// </summary>
    public static async Task<PublicVideoDetailDto> ToPublicVideoDetailDtoAsync(
        this VideoEntity entity,
        IMapper mapper,
        IFileStorageService fileStorage,
        CancellationToken ct = default,
        short? ratedStars = null
    )
    {
        string? thumbnailUrl = await ResolveThumbnailUrlAsync(entity, fileStorage, ct);

        return new PublicVideoDetailDto(
            entity.Id,
            entity.CategoryId,
            entity.Category != null ? entity.Category.Name : string.Empty,
            entity.Title,
            entity.Slug,
            entity.Description,
            thumbnailUrl,
            entity.YoutubeVideoUrl,
            entity.IsPromoted,
            entity.HasLyrics,
            entity.PublishedAt,
            entity.MetaTitle,
            entity.MetaDescription,
            mapper.Map<IReadOnlyList<TagDto>>(entity.Tags),
            entity.ShareCount,
            entity.RatingAverage,
            entity.RatingCount,
            IsRated: ratedStars.HasValue,
            RatedStars: ratedStars
        );
    }

    /// <summary>
    /// Maps a list of <see cref="VideoEntity" /> to a list of <see cref="VideoSummaryDto" />,
    /// resolving thumbnail URLs from associated FileReferenceDto records.
    /// </summary>
    public static async Task<IReadOnlyList<VideoSummaryDto>> ToVideoSummaryDtosAsync(
        this IReadOnlyList<VideoEntity> entities,
        IMapper mapper,
        IFileStorageService fileStorage,
        CancellationToken ct = default
    )
    {
        IReadOnlyDictionary<Guid, FileReferenceDto> files = await fileStorage.ResolveManyAsync(
            entities.Where(e => e.ThumbnailFileId.HasValue).Select(e => e.ThumbnailFileId!.Value).Distinct().ToList(),
            ct
        );

        return entities.Select(entity => entity.ToVideoSummaryDto(mapper, files)).ToList();
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
        IReadOnlyDictionary<Guid, FileReferenceDto> files
    )
    {
        string? thumbnailUrl =
            entity.ThumbnailFileId is { } thumbnailId && files.TryGetValue(thumbnailId, out FileReferenceDto? thumbnail)
                ? thumbnail.StorageUrl
                : null;

        return new VideoSummaryDto(
            entity.Id,
            entity.CategoryId,
            entity.Category != null ? entity.Category.Name : string.Empty,
            entity.Title,
            entity.Slug,
            thumbnailUrl,
            entity.AuthorId.ToString(),
            entity.Status,
            entity.YoutubeVideoUrl,
            entity.IsPromoted,
            entity.HasLyrics,
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
    /// Resolves the thumbnail URL from the associated FileReferenceDto, or returns null
    /// if no thumbnail has been uploaded.
    /// </summary>
    private static async Task<string?> ResolveThumbnailUrlAsync(
        VideoEntity entity,
        IFileStorageService fileStorage,
        CancellationToken ct
    )
    {
        if (!entity.ThumbnailFileId.HasValue)
        {
            return null;
        }

        FileReferenceDto? thumbnailFile = await fileStorage.ResolveAsync(entity.ThumbnailFileId.Value, ct);
        return thumbnailFile?.StorageUrl;
    }
}
