using _116.Content.Application.Shared.DTOs;
using _116.Content.Domain.Entities;
using Mapster;
using MapsterMapper;

namespace _116.Content.Application.Shared.Mappers;

/// <summary>
/// Mapster configuration for PromotionLevel entity mappings.
/// Uses dependency injection instead of global static state.
/// </summary>
public static class PromotionLevelMapper
{
    /// <summary>
    /// Registers PromotionLevel entity mappings into the provided TypeAdapterConfig.
    /// This method does NOT mutate global state.
    /// </summary>
    /// <param name="config">The TypeAdapterConfig to register mappings into.</param>
    public static void Register(TypeAdapterConfig config)
    {
        config.NewConfig<PromotionLevelEntity, PromotionLevelDto>();
    }

    /// <summary>
    /// Maps a <see cref="PromotionLevelEntity" /> to a <see cref="PromotionLevelDto" />.
    /// </summary>
    /// <param name="entity">The promotion level entity to map.</param>
    /// <param name="mapper">Injected IMapper instance.</param>
    /// <returns>A <see cref="PromotionLevelDto" /> containing promotion level information.</returns>
    public static PromotionLevelDto ToPromotionLevelDto(this PromotionLevelEntity entity, IMapper mapper)
    {
        return mapper.Map<PromotionLevelDto>(entity);
    }

    /// <summary>
    /// Maps a collection of <see cref="PromotionLevelEntity" /> to a list of <see cref="PromotionLevelDto" />.
    /// </summary>
    /// <param name="entities">The promotion level entities to map.</param>
    /// <param name="mapper">Injected IMapper instance.</param>
    /// <returns>A read-only list of <see cref="PromotionLevelDto" />.</returns>
    public static IReadOnlyList<PromotionLevelDto> ToPromotionLevelDtos(
        this IReadOnlyList<PromotionLevelEntity> entities,
        IMapper mapper
    )
    {
        return mapper.Map<IReadOnlyList<PromotionLevelDto>>(entities);
    }
}
