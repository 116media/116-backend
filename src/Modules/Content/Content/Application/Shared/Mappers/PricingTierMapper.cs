using _116.Content.Application.Shared.DTOs;
using _116.Content.Domain.Entities;
using Mapster;
using MapsterMapper;

namespace _116.Content.Application.Shared.Mappers;

/// <summary>
/// Mapster configuration for PricingTier entity mappings.
/// Uses dependency injection instead of global static state.
/// </summary>
public static class PricingTierMapper
{
    /// <summary>
    /// Registers PricingTier entity mappings into the provided TypeAdapterConfig.
    /// This method does NOT mutate global state.
    /// </summary>
    /// <param name="config">The TypeAdapterConfig to register mappings into.</param>
    public static void Register(TypeAdapterConfig config)
    {
        config.NewConfig<PricingTierEntity, PricingTierDto>();
    }

    /// <summary>
    /// Maps a <see cref="PricingTierEntity" /> to a <see cref="PricingTierDto" />.
    /// </summary>
    /// <param name="entity">The pricing tier entity to map.</param>
    /// <param name="mapper">Injected IMapper instance.</param>
    /// <returns>A <see cref="PricingTierDto" /> containing pricing tier information.</returns>
    public static PricingTierDto ToPricingTierDto(this PricingTierEntity entity, IMapper mapper)
    {
        return mapper.Map<PricingTierDto>(entity);
    }

    /// <summary>
    /// Maps a collection of <see cref="PricingTierEntity" /> to a list of <see cref="PricingTierDto" />.
    /// </summary>
    /// <param name="entities">The pricing tier entities to map.</param>
    /// <param name="mapper">Injected IMapper instance.</param>
    /// <returns>A read-only list of <see cref="PricingTierDto" />.</returns>
    public static IReadOnlyList<PricingTierDto> ToPricingTierDtos(
        this IReadOnlyList<PricingTierEntity> entities,
        IMapper mapper
    )
    {
        return mapper.Map<IReadOnlyList<PricingTierDto>>(entities);
    }
}
