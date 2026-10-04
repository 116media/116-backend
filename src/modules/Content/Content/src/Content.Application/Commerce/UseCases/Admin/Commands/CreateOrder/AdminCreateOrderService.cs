using _116.Content.Application.Catalog.Specifications;
using _116.Content.Application.Commerce.UseCases.Admin.Commands.CreateOrder.Contracts;
using _116.Content.Application.Shared.Errors.Facade;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;

namespace _116.Content.Application.Commerce.UseCases.Admin.Commands.CreateOrder;

/// <summary>
/// Service that populates a draft order with items and tiers from a package's slots.
/// Every category is priced in a single query, so slot count never drives round-trips.
/// </summary>
/// <param name="customerRepository">Repository resolving the commissioning customer.</param>
/// <param name="packageRepository">Repository resolving the seeding package.</param>
/// <param name="contentOrderRepository">Repository staging the order.</param>
/// <param name="categoryRepository">Repository for category data access operations.</param>
/// <param name="contentTypeRepository">Repository resolving the slot categories' content types.</param>
/// <param name="i18n">Single i18n entry point for the Content module.</param>
public class AdminCreateOrderService(
    ICustomerRepository customerRepository,
    IPackageRepository packageRepository,
    IContentOrderRepository contentOrderRepository,
    ICategoryRepository categoryRepository,
    IContentTypeRepository contentTypeRepository,
    ContentI18n i18n
) : ICreateOrderService
{
    /// <inheritdoc />
    public async Task<CreatedOrderData> CreateAsync(Guid customerId, Guid? packageId, CancellationToken ct)
    {
        CustomerEntity? customer = await customerRepository.GetByIdAsync(id: customerId, cancellationToken: ct);

        if (customer is null)
        {
            throw i18n.Customer.NotFound(id: customerId);
        }

        PackageEntity? package = null;

        if (packageId is { } id)
        {
            package = await packageRepository.GetByIdAsync(id: id, cancellationToken: ct);

            if (package is null || !new ActivePackageSpecification().IsSatisfiedBy(package))
            {
                throw i18n.Package.NotFound(id: id);
            }
        }

        var order = ContentOrderEntity.Create(id: Guid.NewGuid(), customerId: customerId, packageId: packageId);
        await contentOrderRepository.AddAsync(order: order, ct: ct);

        int itemCount = package is null ? 0 : await PopulateFromPackageAsync(order: order, package: package, ct: ct);

        return new CreatedOrderData(Order: order, Customer: customer, ItemCount: itemCount);
    }

    /// <inheritdoc />
    public async Task<int> PopulateFromPackageAsync(
        ContentOrderEntity order,
        PackageEntity package,
        CancellationToken ct
    )
    {
        List<PackageSlotEntity> slotsWithCategory = [.. package.Slots.Where(s => s.CategoryId.HasValue)];
        Guid[] categoryIds = [.. slotsWithCategory.Select(s => s.CategoryId!.Value).Distinct()];

        IReadOnlyDictionary<Guid, CategoryEntity> categories = await categoryRepository.GetByIdsAsync(
            ids: categoryIds,
            cancellationToken: ct
        );

        IReadOnlyDictionary<Guid, ContentTypeEntity> contentTypes = await contentTypeRepository.GetByIdsAsync(
            ids: [.. categories.Values.Select(category => category.ContentTypeId).Distinct()],
            cancellationToken: ct
        );

        Dictionary<Guid, List<CategoryPricingEntity>> pricingByCategory = categories.ToDictionary(
            entry => entry.Key,
            entry => entry.Value.Pricing.ToList()
        );

        List<ContentOrderItemEntity> items =
        [
            .. slotsWithCategory
                .Select(slot =>
                    (
                        Slot: slot,
                        ContentKind: ResolveContentKind(slot, categories, contentTypes),
                        Pricing: pricingByCategory.GetValueOrDefault(slot.CategoryId!.Value) ?? []
                    )
                )
                .SelectMany(resolved =>
                    Enumerable
                        .Range(0, resolved.Slot.Quantity)
                        .Select(_ =>
                            CreateItem(
                                order: order,
                                slot: resolved.Slot,
                                contentKind: resolved.ContentKind,
                                pricing: resolved.Pricing
                            )
                        )
                ),
        ];

        return items.Count;
    }

    /// <summary>
    /// Maps a slot's content-type name onto the content-kind enum, falling back to
    /// <see cref="EnumCoreContentType.Custom" /> for a name the enum does not declare.
    /// </summary>
    /// <param name="slot">The package slot being filled.</param>
    /// <param name="categories">The slot categories, keyed by id.</param>
    /// <param name="contentTypes">The categories' content types, keyed by id.</param>
    /// <returns>The resolved content kind.</returns>
    private static EnumCoreContentType ResolveContentKind(
        PackageSlotEntity slot,
        IReadOnlyDictionary<Guid, CategoryEntity> categories,
        IReadOnlyDictionary<Guid, ContentTypeEntity> contentTypes
    )
    {
        if (
            !categories.TryGetValue(slot.CategoryId!.Value, out CategoryEntity? category)
            || !contentTypes.TryGetValue(category.ContentTypeId, out ContentTypeEntity? contentType)
        )
        {
            return EnumCoreContentType.Custom;
        }

        return Enum.TryParse(contentType.Name, ignoreCase: true, out EnumCoreContentType contentKind)
            ? contentKind
            : EnumCoreContentType.Custom;
    }

    /// <summary>
    /// Builds one order item for a slot, with a tier snapshotting each of its category's prices.
    /// </summary>
    /// <param name="order">The order the item belongs to.</param>
    /// <param name="slot">The slot being filled; a non-required slot yields a bonus item.</param>
    /// <param name="contentKind">The resolved content kind.</param>
    /// <param name="pricing">The category's pricing rows to snapshot.</param>
    /// <returns>The item, with its tiers attached.</returns>
    private static ContentOrderItemEntity CreateItem(
        ContentOrderEntity order,
        PackageSlotEntity slot,
        EnumCoreContentType contentKind,
        IReadOnlyList<CategoryPricingEntity> pricing
    )
    {
        ContentOrderItemEntity item = order.AddItem(
            contentKind: contentKind,
            categoryId: slot.CategoryId!.Value,
            promotionLevelId: null,
            promoPriceSnapshotUsd: null,
            socialBoost: false,
            isBonus: !slot.IsRequired
        );

        foreach (CategoryPricingEntity price in pricing)
        {
            order.AddTier(item: item, pricingTierId: price.PricingTierId, priceSnapshotUsd: price.PriceUsd);
        }

        return item;
    }
}
