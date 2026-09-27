using _116.Content.Domain.Entities;

namespace _116.Content.Application.Shared.Mappers;

/// <summary>
/// The rows an order projection names but does not own — resolved in one batch per request so
/// the mapper reads names without a navigation or a per-row query.
/// </summary>
/// <param name="Customers">Ordering customers, keyed by id.</param>
/// <param name="Categories">Item categories, keyed by id.</param>
/// <param name="PromotionLevels">Purchased promotion levels, keyed by id.</param>
/// <param name="PricingTiers">Purchased pricing tiers, keyed by id.</param>
public record OrderLookups(
    IReadOnlyDictionary<Guid, CustomerEntity> Customers,
    IReadOnlyDictionary<Guid, CategoryEntity> Categories,
    IReadOnlyDictionary<Guid, PromotionLevelEntity> PromotionLevels,
    IReadOnlyDictionary<Guid, PricingTierEntity> PricingTiers
);
