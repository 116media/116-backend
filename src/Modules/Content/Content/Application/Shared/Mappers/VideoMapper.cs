using _116.Content.Application.Shared.DTOs;
using _116.Content.Domain.Entities;
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
    /// Maps a <see cref="VideoEntity" /> to a <see cref="VideoSummaryDto" />.
    /// </summary>
    public static VideoSummaryDto ToVideoSummaryDto(this VideoEntity entity, IMapper mapper)
    {
        return new VideoSummaryDto(
            entity.Id,
            entity.CategoryId,
            entity.Category != null ? entity.Category.Name : string.Empty,
            entity.Title,
            entity.Slug,
            entity.ThumbnailUrl,
            entity.AuthorId.ToString(),
            entity.Status,
            entity.YoutubeVideoUrl,
            entity.IsPromoted,
            entity.HasLyrics,
            entity.PublishedAt,
            entity.ShootingScheduledAt
        )
        {
            CreatedAt = entity.CreatedAt,
            CreatedBy = entity.CreatedBy,
            UpdatedAt = entity.UpdatedAt,
            UpdatedBy = entity.UpdatedBy,
        };
    }

    /// <summary>
    /// Maps a <see cref="VideoEntity" /> to a <see cref="VideoDetailDto" />.
    /// </summary>
    public static VideoDetailDto ToVideoDetailDto(this VideoEntity entity, IMapper mapper)
    {
        return new VideoDetailDto(
            entity.Id,
            entity.CategoryId,
            entity.Category != null ? entity.Category.Name : string.Empty,
            entity.Title,
            entity.Slug,
            entity.Description,
            entity.ThumbnailUrl,
            entity.ThumbnailStorageKey,
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
            entity.CustomerId,
            entity.Customer != null ? entity.Customer.FullName : null,
            entity.OrderItemId
        )
        {
            CreatedAt = entity.CreatedAt,
            CreatedBy = entity.CreatedBy,
            UpdatedAt = entity.UpdatedAt,
            UpdatedBy = entity.UpdatedBy,
        };
    }

    /// <summary>
    /// Maps a list of <see cref="VideoEntity" /> to a list of <see cref="VideoSummaryDto" />.
    /// </summary>
    public static IReadOnlyList<VideoSummaryDto> ToVideoSummaryDtos(
        this IReadOnlyList<VideoEntity> entities,
        IMapper mapper
    )
    {
        return entities.Select(e => e.ToVideoSummaryDto(mapper)).ToList();
    }
}
