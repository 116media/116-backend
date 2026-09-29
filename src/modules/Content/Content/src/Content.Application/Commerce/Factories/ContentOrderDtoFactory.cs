using _116.Content.Application.Shared.DTOs;
using _116.Content.Application.Shared.Mappers;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using MapsterMapper;

namespace _116.Content.Application.Commerce.Factories;

/// <summary>
/// Factory implementation building order projections from pre-resolved lookup maps.
/// </summary>
/// <param name="mapper">Injected IMapper instance.</param>
/// <param name="customerRepository">Repository resolving ordering customers.</param>
/// <param name="categoryRepository">Repository resolving item categories.</param>
/// <param name="promotionLevelRepository">Repository resolving purchased promotion levels.</param>
/// <param name="pricingTierRepository">Repository resolving purchased pricing tiers.</param>
public class ContentOrderDtoFactory(
    IMapper mapper,
    ICustomerRepository customerRepository,
    ICategoryRepository categoryRepository,
    IPromotionLevelRepository promotionLevelRepository,
    IPricingTierRepository pricingTierRepository
) : IContentOrderDtoFactory
{
    /// <inheritdoc />
    public async Task<ContentOrderSummaryDto> CreateSummaryAsync(
        ContentOrderEntity order,
        CancellationToken ct = default
    )
    {
        IReadOnlyDictionary<Guid, CustomerEntity> customers = await ResolveCustomersAsync([order], ct);

        return order.ToContentOrderSummaryDto(mapper, customers);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<ContentOrderSummaryDto>> CreateManySummariesAsync(
        IReadOnlyList<ContentOrderEntity> orders,
        CancellationToken ct = default
    )
    {
        IReadOnlyDictionary<Guid, CustomerEntity> customers = await ResolveCustomersAsync(orders, ct);

        return orders.ToContentOrderSummaryDtos(mapper, customers);
    }

    /// <inheritdoc />
    public async Task<ContentOrderDetailDto> CreateDetailAsync(ContentOrderEntity order, CancellationToken ct = default)
    {
        OrderLookups lookups = await ResolveLookupsAsync([order], ct);

        return order.ToContentOrderDetailDto(mapper, lookups);
    }

    /// <inheritdoc />
    public async Task<OrderItemDto> CreateItemAsync(ContentOrderItemEntity item, CancellationToken ct = default)
    {
        OrderLookups lookups = await ResolveItemLookupsAsync([item], ct);

        return item.ToOrderItemDto(mapper, lookups);
    }

    /// <inheritdoc />
    public Task<IReadOnlyDictionary<Guid, CustomerEntity>> ResolveCustomersAsync(
        IReadOnlyList<ContentOrderEntity> orders,
        CancellationToken ct = default
    )
    {
        return customerRepository.GetByIdsAsync(
            ids: orders.Select(order => order.CustomerId).Distinct().ToList(),
            cancellationToken: ct
        );
    }

    /// <inheritdoc />
    public async Task<OrderLookups> ResolveLookupsAsync(
        IReadOnlyList<ContentOrderEntity> orders,
        CancellationToken ct = default
    )
    {
        OrderLookups itemLookups = await ResolveItemLookupsAsync([.. orders.SelectMany(order => order.Items)], ct);
        IReadOnlyDictionary<Guid, CustomerEntity> customers = await ResolveCustomersAsync(orders, ct);

        return itemLookups with
        {
            Customers = customers,
        };
    }

    /// <summary>
    /// Resolves the categories, promotion levels and pricing tiers a set of items names.
    /// The customer map is left empty: items carry no customer of their own.
    /// </summary>
    /// <param name="items">The items to resolve lookups for.</param>
    /// <param name="ct">Token to observe for cancellation requests.</param>
    /// <returns>The resolved lookups.</returns>
    private async Task<OrderLookups> ResolveItemLookupsAsync(
        IReadOnlyList<ContentOrderItemEntity> items,
        CancellationToken ct
    )
    {
        IReadOnlyDictionary<Guid, CategoryEntity> categories = await categoryRepository.GetByIdsAsync(
            ids: items.Select(item => item.CategoryId).Distinct().ToList(),
            cancellationToken: ct
        );

        IReadOnlyDictionary<Guid, PromotionLevelEntity> promotionLevels = await promotionLevelRepository.GetByIdsAsync(
            ids: items
                .Where(item => item.PromotionLevelId.HasValue)
                .Select(item => item.PromotionLevelId!.Value)
                .Distinct()
                .ToList(),
            cancellationToken: ct
        );

        IReadOnlyDictionary<Guid, PricingTierEntity> pricingTiers = await pricingTierRepository.GetByIdsAsync(
            ids: items.SelectMany(item => item.Tiers).Select(tier => tier.PricingTierId).Distinct().ToList(),
            cancellationToken: ct
        );

        return new OrderLookups(
            Customers: new Dictionary<Guid, CustomerEntity>(),
            Categories: categories,
            PromotionLevels: promotionLevels,
            PricingTiers: pricingTiers
        );
    }
}
