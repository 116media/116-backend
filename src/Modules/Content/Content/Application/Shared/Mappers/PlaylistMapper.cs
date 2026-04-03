using _116.Content.Application.Shared.DTOs;
using _116.Content.Domain.Entities;
using Mapster;
using MapsterMapper;

namespace _116.Content.Application.Shared.Mappers;

/// <summary>
/// Mapster configuration for Playlist entity mappings.
/// </summary>
public static class PlaylistMapper
{
    /// <summary>
    /// Registers Playlist entity mappings into the provided TypeAdapterConfig.
    /// </summary>
    /// <param name="config">The TypeAdapterConfig to register mappings into.</param>
    public static void Register(TypeAdapterConfig config)
    {
        config
            .NewConfig<PlaylistVideoEntity, VideoInPlaylistDto>()
            .Map(dest => dest.VideoId, src => src.VideoId)
            .Map(dest => dest.Title, src => src.Video.Title)
            .Map(dest => dest.ThumbnailUrl, src => src.Video.ThumbnailUrl)
            .Map(dest => dest.RatingAverage, src => src.Video.RatingAverage)
            .Map(dest => dest.RatingCount, src => src.Video.RatingCount)
            .Map(dest => dest.SortOrder, src => src.SortOrder);

        config.NewConfig<PlaylistEntity, PlaylistDto>().Map(dest => dest.VideoCount, src => src.Videos.Count);

        config
            .NewConfig<PlaylistEntity, PlaylistDetailDto>()
            .Map(dest => dest.Videos, src => src.Videos.OrderBy(v => v.SortOrder).ToList());
    }

    /// <summary>
    /// Maps a <see cref="PlaylistEntity" /> to a <see cref="PlaylistDetailDto" />.
    /// </summary>
    public static PlaylistDetailDto ToPlaylistDetailDto(this PlaylistEntity entity, IMapper mapper)
    {
        var dto = mapper.Map<PlaylistDetailDto>(entity);
        return dto with
        {
            Videos = mapper.Map<IReadOnlyList<VideoInPlaylistDto>>(entity.Videos.OrderBy(v => v.SortOrder).ToList()),
        };
    }

    /// <summary>
    /// Maps a list of <see cref="PlaylistEntity" /> to a list of <see cref="PlaylistDto" />.
    /// </summary>
    public static IReadOnlyList<PlaylistDto> ToPlaylistDtos(this IReadOnlyList<PlaylistEntity> entities, IMapper mapper)
    {
        return entities
            .Select(e =>
            {
                var dto = mapper.Map<PlaylistDto>(e);
                return dto with { VideoCount = e.Videos.Count };
            })
            .ToList();
    }
}
