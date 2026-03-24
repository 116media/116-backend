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
    /// <param name="config">The TypeAdapterConfig to register mappings into.</param>
    public static void Register(TypeAdapterConfig config)
    {
        config
            .NewConfig<VideoTagEntity, TagDto>()
            .Map(dest => dest.Id, src => src.Tag.Id)
            .Map(dest => dest.Name, src => src.Tag.Name)
            .Map(dest => dest.Slug, src => src.Tag.Slug);

        config.NewConfig<VideoEntity, VideoSummaryDto>().Map(dest => dest.CategoryName, src => src.Category.Name);

        config
            .NewConfig<VideoEntity, VideoDetailDto>()
            .Map(dest => dest.CategoryName, src => src.Category.Name)
            .Map(dest => dest.Tags, src => src.Tags);
    }

    /// <summary>
    /// Maps a <see cref="VideoEntity" /> to a <see cref="VideoSummaryDto" />.
    /// </summary>
    public static VideoSummaryDto ToVideoSummaryDto(this VideoEntity entity, IMapper mapper)
    {
        return mapper.Map<VideoSummaryDto>(entity);
    }

    /// <summary>
    /// Maps a <see cref="VideoEntity" /> to a <see cref="VideoDetailDto" />.
    /// </summary>
    public static VideoDetailDto ToVideoDetailDto(this VideoEntity entity, IMapper mapper)
    {
        return mapper.Map<VideoDetailDto>(entity);
    }

    /// <summary>
    /// Maps a list of <see cref="VideoEntity" /> to a list of <see cref="VideoSummaryDto" />.
    /// </summary>
    public static IReadOnlyList<VideoSummaryDto> ToVideoSummaryDtos(
        this IReadOnlyList<VideoEntity> entities,
        IMapper mapper
    )
    {
        return mapper.Map<IReadOnlyList<VideoSummaryDto>>(entities);
    }
}
