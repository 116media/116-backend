using _116.Content.Application.Shared.DTOs;
using _116.Content.Domain.Entities;
using Mapster;
using MapsterMapper;

namespace _116.Content.Application.Shared.Mappers;

/// <summary>
/// Mapster configuration for Package and PackageSlot entity mappings.
/// </summary>
public static class PackageMapper
{
    /// <summary>
    /// Registers Package and PackageSlot entity mappings into the provided TypeAdapterConfig.
    /// </summary>
    /// <param name="config">The TypeAdapterConfig to register mappings into.</param>
    public static void Register(TypeAdapterConfig config)
    {
        config.NewConfig<PackageSlotEntity, PackageSlotDto>().Map(dest => dest.CategoryName, _ => (string?)null);

        config
            .NewConfig<PackageEntity, PackageDto>()
            .Map(dest => dest.Slots, src => src.Slots)
            .Map(dest => dest.CalculatedPriceUsd, _ => 0m);
    }

    /// <summary>
    /// Maps a <see cref="PackageEntity" /> to a <see cref="PackageDto" />, reading each slot's
    /// category name and price from a pre-fetched map. Performs no IO.
    /// </summary>
    public static PackageDto ToPackageDto(
        this PackageEntity entity,
        IMapper mapper,
        IReadOnlyDictionary<Guid, CategoryEntity> categories
    )
    {
        var dto = mapper.Map<PackageDto>(entity);

        decimal calculatedPrice = entity
            .Slots.Where(slot => slot.IsRequired)
            .Sum(slot =>
                SlotCategory(slot, categories) is { } category
                    ? category.Pricing.Sum(pricing => pricing.PriceUsd) * slot.Quantity
                    : 0m
            );

        return dto with
        {
            Slots = [.. entity.Slots.Select(slot => slot.ToPackageSlotDto(mapper, categories))],
            CalculatedPriceUsd = calculatedPrice,
        };
    }

    /// <summary>
    /// Maps a collection of <see cref="PackageEntity" /> to a list of <see cref="PackageDto" />.
    /// </summary>
    public static IReadOnlyList<PackageDto> ToPackageDtos(
        this IReadOnlyList<PackageEntity> entities,
        IMapper mapper,
        IReadOnlyDictionary<Guid, CategoryEntity> categories
    )
    {
        return entities.Select(entity => entity.ToPackageDto(mapper, categories)).ToList();
    }

    /// <summary>
    /// Maps a <see cref="PackageSlotEntity" /> to a <see cref="PackageSlotDto" />, reading the
    /// category name from a pre-fetched map. Performs no IO.
    /// </summary>
    public static PackageSlotDto ToPackageSlotDto(
        this PackageSlotEntity entity,
        IMapper mapper,
        IReadOnlyDictionary<Guid, CategoryEntity> categories
    )
    {
        var dto = mapper.Map<PackageSlotDto>(entity);

        return dto with
        {
            CategoryName = SlotCategory(entity, categories)?.Name,
        };
    }

    /// <summary>
    /// Reads a slot's category out of a resolved map; null for an open slot or a category row
    /// that no longer exists.
    /// </summary>
    /// <param name="slot">The slot whose category to read.</param>
    /// <param name="categories">The resolved categories, keyed by id.</param>
    /// <returns>The category, or <c>null</c>.</returns>
    private static CategoryEntity? SlotCategory(
        PackageSlotEntity slot,
        IReadOnlyDictionary<Guid, CategoryEntity> categories
    )
    {
        return slot.CategoryId is { } categoryId && categories.TryGetValue(categoryId, out CategoryEntity? category)
            ? category
            : null;
    }
}
