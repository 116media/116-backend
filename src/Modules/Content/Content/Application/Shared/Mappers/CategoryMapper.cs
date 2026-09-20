using _116.Content.Application.Shared.DTOs;
using _116.Content.Domain.Entities;
using _116.Core.Contracts.Application.DTOs;
using Mapster;
using MapsterMapper;

namespace _116.Content.Application.Shared.Mappers;

/// <summary>
/// Mapster configuration for Category and CategoryPricing entity mappings. Poster URLs come from a
/// pre-fetched file map rather than the entity, so every projection here is synchronous and IO-free.
/// </summary>
public static class CategoryMapper
{
    /// <summary>
    /// Registers Category and CategoryPricing entity mappings into the provided TypeAdapterConfig.
    /// Ignores <c>PosterUrl</c> since it is resolved at mapping time from the associated FileEntity.
    /// </summary>
    /// <param name="config">The TypeAdapterConfig to register mappings into.</param>
    public static void Register(TypeAdapterConfig config)
    {
        config
            .NewConfig<CategoryPricingEntity, CategoryPricingDto>()
            .Map(dest => dest.TierId, src => src.PricingTierId)
            .Map(dest => dest.TierName, _ => string.Empty)
            .Map(dest => dest.PriceUsd, src => src.PriceUsd);

        config
            .NewConfig<CategoryEntity, CategoryDto>()
            .Map(dest => dest.ContentTypeName, _ => string.Empty)
            .Map(dest => dest.PosterUrl, _ => (string?)null)
            .Map(dest => dest.Colors, _ => (CategoryColorsDto?)null)
            .Map(dest => dest.Pricing, src => src.Pricing);
    }

    /// <summary>
    /// Reads the background/foreground color pair from a poster file. Pure
    /// pass-through — the colors were computed once at upload time and stored on
    /// the file, so listing many categories costs no per-card computation.
    /// </summary>
    /// <param name="posterFile">The poster file, or null when the category has no poster.</param>
    /// <returns>The color pair, or null when the file carries no extracted colors.</returns>
    private static CategoryColorsDto? ResolveColors(FileReferenceDto? posterFile) =>
        posterFile?.DominantColorHex is { } background && posterFile.ForegroundColorHex is { } foreground
            ? new CategoryColorsDto(background, foreground)
            : null;

    /// <summary>
    /// Maps a <see cref="CategoryEntity" /> to a <see cref="CategoryDto" />, resolving the poster
    /// URL from a pre-fetched file map. Performs no IO — intended for batch mapping (e.g. the
    /// content feed) where files are loaded once up front via <c>IFileRepository.GetByIdsAsync</c>.
    /// </summary>
    public static CategoryDto ToCategoryDto(this CategoryEntity entity, IMapper mapper, CategoryLookups lookups)
    {
        var dto = mapper.Map<CategoryDto>(entity);

        string? posterUrl = null;
        CategoryColorsDto? colors = null;
        if (entity.PosterFileId is { } posterId && lookups.Posters.TryGetValue(posterId, out FileReferenceDto? poster))
        {
            posterUrl = poster.StorageUrl;
            colors = ResolveColors(poster);
        }

        return dto with
        {
            ContentTypeName = lookups.ContentTypes.TryGetValue(entity.ContentTypeId, out ContentTypeEntity? contentType)
                ? contentType.Name
                : string.Empty,
            PosterUrl = posterUrl,
            Colors = colors,
            Pricing =
            [
                .. entity.Pricing.Select(pricing =>
                    pricing.ToCategoryPricingDto(mapper, lookups.PricingTiers.GetValueOrDefault(pricing.PricingTierId))
                ),
            ],
        };
    }

    /// <summary>
    /// Maps a <see cref="CategoryPricingEntity" /> to a <see cref="CategoryPricingDto" />, reading
    /// the tier name from a pre-fetched map. Performs no IO.
    /// </summary>
    public static CategoryPricingDto ToCategoryPricingDto(
        this CategoryPricingEntity entity,
        IMapper mapper,
        PricingTierEntity? pricingTier
    )
    {
        var dto = mapper.Map<CategoryPricingDto>(entity);

        return dto with
        {
            TierName = pricingTier is null ? string.Empty : pricingTier.Name,
        };
    }
}
