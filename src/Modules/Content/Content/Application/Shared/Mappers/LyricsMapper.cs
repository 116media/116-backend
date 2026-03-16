using _116.Content.Application.Shared.DTOs;
using _116.Content.Domain.Entities;
using Mapster;
using MapsterMapper;

namespace _116.Content.Application.Shared.Mappers;

/// <summary>
/// Mapster configuration for Lyrics entity mappings.
/// </summary>
public static class LyricsMapper
{
    /// <summary>
    /// Registers Lyrics entity mappings into the provided TypeAdapterConfig.
    /// </summary>
    /// <param name="config">The TypeAdapterConfig to register mappings into.</param>
    public static void Register(TypeAdapterConfig config)
    {
        config.NewConfig<LyricsEntity, LyricsDto>();
    }

    /// <summary>
    /// Maps a <see cref="LyricsEntity" /> to a <see cref="LyricsDto" />.
    /// </summary>
    public static LyricsDto ToLyricsDto(this LyricsEntity entity, IMapper mapper)
    {
        return mapper.Map<LyricsDto>(entity);
    }

    /// <summary>
    /// Maps a list of <see cref="LyricsEntity" /> to a list of <see cref="LyricsDto" />.
    /// </summary>
    public static IReadOnlyList<LyricsDto> ToLyricsDtos(this IReadOnlyList<LyricsEntity> entities, IMapper mapper)
    {
        return mapper.Map<IReadOnlyList<LyricsDto>>(entities);
    }
}
