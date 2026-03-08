using _116.Content.Application.Shared.DTOs;
using _116.Content.Domain.Entities;
using Mapster;
using MapsterMapper;

namespace _116.Content.Application.Shared.Mappers;

/// <summary>
/// Mapster configuration for Tag entity mappings.
/// Uses dependency injection instead of global static state.
/// </summary>
public static class TagMapper
{
    /// <summary>
    /// Registers Tag entity mappings into the provided TypeAdapterConfig.
    /// This method does NOT mutate global state.
    /// </summary>
    /// <param name="config">The TypeAdapterConfig to register mappings into.</param>
    public static void Register(TypeAdapterConfig config)
    {
        config.NewConfig<TagEntity, TagDto>();
    }

    /// <summary>
    /// Maps a <see cref="TagEntity" /> to a <see cref="TagDto" />.
    /// </summary>
    /// <param name="entity">The tag entity to map.</param>
    /// <param name="mapper">Injected IMapper instance.</param>
    /// <returns>A <see cref="TagDto" /> containing tag information.</returns>
    public static TagDto ToTagDto(this TagEntity entity, IMapper mapper)
    {
        return mapper.Map<TagDto>(entity);
    }

    /// <summary>
    /// Maps a collection of <see cref="TagEntity" /> to a list of <see cref="TagDto" />.
    /// </summary>
    /// <param name="entities">The tag entities to map.</param>
    /// <param name="mapper">Injected IMapper instance.</param>
    /// <returns>A read-only list of <see cref="TagDto" />.</returns>
    public static IReadOnlyList<TagDto> ToTagDtos(this IReadOnlyList<TagEntity> entities, IMapper mapper)
    {
        return mapper.Map<IReadOnlyList<TagDto>>(entities);
    }
}
