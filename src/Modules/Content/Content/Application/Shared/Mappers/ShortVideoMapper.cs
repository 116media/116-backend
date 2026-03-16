using _116.Content.Application.Shared.DTOs;
using _116.Content.Domain.Entities;
using Mapster;
using MapsterMapper;

namespace _116.Content.Application.Shared.Mappers;

/// <summary>
/// Mapster configuration for ShortVideo entity mappings.
/// </summary>
public static class ShortVideoMapper
{
    /// <summary>
    /// Registers ShortVideo entity mappings into the provided TypeAdapterConfig.
    /// </summary>
    /// <param name="config">The TypeAdapterConfig to register mappings into.</param>
    public static void Register(TypeAdapterConfig config)
    {
        config.NewConfig<ShortVideoEntity, ShortVideoDto>();
    }

    /// <summary>
    /// Maps a <see cref="ShortVideoEntity" /> to a <see cref="ShortVideoDto" />.
    /// </summary>
    public static ShortVideoDto ToShortVideoDto(this ShortVideoEntity entity, IMapper mapper)
    {
        return mapper.Map<ShortVideoDto>(entity);
    }

    /// <summary>
    /// Maps a list of <see cref="ShortVideoEntity" /> to a list of <see cref="ShortVideoDto" />.
    /// </summary>
    public static IReadOnlyList<ShortVideoDto> ToShortVideoDtos(
        this IReadOnlyList<ShortVideoEntity> entities,
        IMapper mapper
    )
    {
        return mapper.Map<IReadOnlyList<ShortVideoDto>>(entities);
    }
}
