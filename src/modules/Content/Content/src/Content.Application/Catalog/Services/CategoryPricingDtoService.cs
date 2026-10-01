using _116.Content.Application.Shared.DTOs;
using _116.Content.Application.Shared.Mappers;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using MapsterMapper;

namespace _116.Content.Application.Catalog.Services;

/// <summary>
/// Assembles category pricing DTOs, resolving the pricing tiers in one batch.
/// </summary>
/// <param name="mapper">The Mapster mapper.</param>
/// <param name="pricingTierRepository">Repository resolving the tiers the rows name.</param>
public class CategoryPricingDtoService(IMapper mapper, IPricingTierRepository pricingTierRepository)
    : ICategoryPricingDtoService
{
    /// <inheritdoc />
    public async Task<CategoryPricingDto> CreateAsync(CategoryPricingEntity pricing, CancellationToken ct = default)
    {
        PricingTierEntity tier = await pricingTierRepository.GetByIdOrThrowAsync(
            id: pricing.PricingTierId,
            cancellationToken: ct
        );

        return pricing.ToCategoryPricingDto(mapper, tier);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<CategoryPricingDto>> CreateManyAsync(
        IEnumerable<CategoryPricingEntity> pricing,
        CancellationToken ct = default
    )
    {
        IReadOnlyDictionary<Guid, PricingTierEntity> tiers = await pricingTierRepository.GetByIdsAsync(
            ids: [.. pricing.Select(row => row.PricingTierId).Distinct()],
            cancellationToken: ct
        );

        return
        [
            .. pricing
                .OrderBy(row => row.PricingTierId)
                .Select(row => row.ToCategoryPricingDto(mapper, tiers.GetValueOrDefault(row.PricingTierId))),
        ];
    }
}
