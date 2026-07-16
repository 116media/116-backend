using _116.Content.Application.Shared.DTOs;
using _116.Content.Domain.Entities;
using _116.Core.Application.Shared.Repositories;
using _116.Core.Domain.Entities;
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
    /// Maps a <see cref="VideoEntity" /> to a <see cref="VideoSummaryDto" />,
    /// resolving the thumbnail URL from the associated FileEntity.
    /// </summary>
    public static async Task<VideoSummaryDto> ToVideoSummaryDtoAsync(
        this VideoEntity entity,
        IMapper mapper,
        IFileRepository fileRepository,
        CancellationToken ct = default
    )
    {
        string? thumbnailUrl = await ResolveThumbnailUrlAsync(entity, fileRepository, ct);

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
    /// Maps a <see cref="VideoEntity" /> to a <see cref="VideoDetailDto" />,
    /// resolving the thumbnail URL from the associated FileEntity.
    /// </summary>
    public static async Task<VideoDetailDto> ToVideoDetailDtoAsync(
        this VideoEntity entity,
        IMapper mapper,
        IFileRepository fileRepository,
        CancellationToken ct = default,
        short? ratedStars = null
    )
    {
        string? thumbnailUrl = await ResolveThumbnailUrlAsync(entity, fileRepository, ct);

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
    /// Maps a list of <see cref="VideoEntity" /> to a list of <see cref="VideoSummaryDto" />,
    /// resolving thumbnail URLs from associated FileEntity records.
    /// </summary>
    public static async Task<IReadOnlyList<VideoSummaryDto>> ToVideoSummaryDtosAsync(
        this IReadOnlyList<VideoEntity> entities,
        IMapper mapper,
        IFileRepository fileRepository,
        CancellationToken ct = default
    )
    {
        var results = new List<VideoSummaryDto>(entities.Count);
        foreach (VideoEntity entity in entities)
        {
            results.Add(await entity.ToVideoSummaryDtoAsync(mapper, fileRepository, ct));
        }
        return results;
    }

    /// <summary>
    /// Maps a list of <see cref="VideoEntity" /> to <see cref="VideoSummaryDto" /> using a
    /// pre-fetched file map. Performs no IO — intended for batch mapping where files are loaded
    /// once up front via <c>IFileRepository.GetByIdsAsync</c>.
    /// </summary>
    public static IReadOnlyList<VideoSummaryDto> ToVideoSummaryDtos(
        this IReadOnlyList<VideoEntity> entities,
        IMapper mapper,
        IReadOnlyDictionary<Guid, FileEntity> files
    )
    {
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
        IReadOnlyDictionary<Guid, FileEntity> files
    )
    {
        string? thumbnailUrl =
            entity.ThumbnailFileId is { } thumbnailId && files.TryGetValue(thumbnailId, out FileEntity? thumbnail)
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
    /// Resolves the thumbnail URL from the associated FileEntity, or returns null
    /// if no thumbnail has been uploaded.
    /// </summary>
    private static async Task<string?> ResolveThumbnailUrlAsync(
        VideoEntity entity,
        IFileRepository fileRepository,
        CancellationToken ct
    )
    {
        if (!entity.ThumbnailFileId.HasValue)
        {
            return null;
        }

        FileEntity? thumbnailFile = await fileRepository.GetByIdAsync(entity.ThumbnailFileId.Value, ct);
        return thumbnailFile?.StorageUrl;
    }
}
