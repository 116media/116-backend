using _116.Content.Application.Shared.DTOs;
using _116.Content.Domain.Entities;
using Mapster;
using MapsterMapper;

namespace _116.Content.Application.Shared.Mappers;

/// <summary>
/// Mapster configuration for Category and CategoryPricing entity mappings.
/// </summary>
public static class CategoryMapper
{
    /// <summary>
    /// Registers Category and CategoryPricing entity mappings into the provided TypeAdapterConfig.
    /// </summary>
    /// <param name="config">The TypeAdapterConfig to register mappings into.</param>
    public static void Register(TypeAdapterConfig config)
    {
        config
            .NewConfig<CategoryPricingEntity, CategoryPricingDto>()
            .Map(dest => dest.TierId, src => src.PricingTierId)
            .Map(dest => dest.TierName, src => src.PricingTier.Name)
            .Map(dest => dest.PriceUsd, src => src.PriceUsd);

        config
            .NewConfig<CategoryEntity, CategoryDto>()
            .Map(dest => dest.ContentTypeName, src => src.ContentType.Name)
            .Map(dest => dest.Pricing, src => src.Pricing);
    }

    /// <summary>Maps a <see cref="CategoryEntity" /> to a <see cref="CategoryDto" />.</summary>
    public static CategoryDto ToCategoryDto(this CategoryEntity entity, IMapper mapper)
    {
        return mapper.Map<CategoryDto>(entity);
    }

    /// <summary>Maps a collection of <see cref="CategoryEntity" /> to a list of <see cref="CategoryDto" />.</summary>
    public static IReadOnlyList<CategoryDto> ToCategoryDtos(this IReadOnlyList<CategoryEntity> entities, IMapper mapper)
    {
        return mapper.Map<IReadOnlyList<CategoryDto>>(entities);
    }

    /// <summary>Maps a <see cref="CategoryPricingEntity" /> to a <see cref="CategoryPricingDto" />.</summary>
    public static CategoryPricingDto ToCategoryPricingDto(this CategoryPricingEntity entity, IMapper mapper)
    {
        return mapper.Map<CategoryPricingDto>(entity);
    }
}
