using _116.Content.Domain.Entities;

namespace _116.Content.Application.Shared.Mappers;

/// <summary>
/// The rows an article, video or lyrics projection names but does not own — resolved in one
/// batch per request so the mapper reads names without a navigation or a per-row query.
/// </summary>
/// <param name="Categories">The categories content is filed under, keyed by id.</param>
/// <param name="Customers">The customers who commissioned paid content, keyed by id.</param>
/// <param name="PromotionLevels">The purchased promotion levels, keyed by id.</param>
/// <param name="Tags">The tags applied through the junction rows, keyed by id.</param>
public record ContentLookups(
    IReadOnlyDictionary<Guid, CategoryEntity> Categories,
    IReadOnlyDictionary<Guid, CustomerEntity> Customers,
    IReadOnlyDictionary<Guid, PromotionLevelEntity> PromotionLevels,
    IReadOnlyDictionary<Guid, TagEntity> Tags
)
{
    /// <summary>
    /// Lookups resolving nothing, for a projection whose names are supplied another way.
    /// </summary>
    public static ContentLookups Empty { get; } =
        new(
            Categories: new Dictionary<Guid, CategoryEntity>(),
            Customers: new Dictionary<Guid, CustomerEntity>(),
            PromotionLevels: new Dictionary<Guid, PromotionLevelEntity>(),
            Tags: new Dictionary<Guid, TagEntity>()
        );

    /// <summary>
    /// The category's display name, or an empty name when the row is gone.
    /// </summary>
    /// <param name="categoryId">The category to name.</param>
    public string CategoryName(Guid categoryId)
    {
        return Categories.TryGetValue(categoryId, out CategoryEntity? category) ? category.Name : string.Empty;
    }

    /// <summary>
    /// The customer's display name, or null when the content carries no customer.
    /// </summary>
    /// <param name="customerId">The customer to name, if any.</param>
    public string? CustomerName(Guid? customerId)
    {
        if (customerId is not { } id || !Customers.TryGetValue(id, out CustomerEntity? customer))
        {
            return null;
        }

        return customer.FullName;
    }

    /// <summary>
    /// The promotion level's name, or null when the content carries no promotion.
    /// </summary>
    /// <param name="promotionLevelId">The promotion level to name, if any.</param>
    public string? PromotionLevelName(Guid? promotionLevelId)
    {
        if (promotionLevelId is not { } id || !PromotionLevels.TryGetValue(id, out PromotionLevelEntity? level))
        {
            return null;
        }

        return level.Name;
    }
}
