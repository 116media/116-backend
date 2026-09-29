using _116.Content.Domain.Entities;

namespace _116.Content.Application.Commerce.UseCases.Admin.Commands.AddItemTier.Contracts;

/// <summary>
/// Factory for attaching a pricing tier snapshot to an order item.
/// Validates draft status, resolves category pricing, creates and persists the tier,
/// and recalculates the order total.
/// </summary>
public interface IAddItemTierFactory
{
    /// <summary>
    /// Fetches order+item, validates Draft status, resolves pricing, creates
    /// ContentItemTierEntity, recalculates order total, and commits.
    /// Returns the created tier entity together with the tier name needed for the response DTO.
    /// </summary>
    Task<(ContentItemTierEntity Tier, string TierName)> AttachTierAsync(
        Guid orderId,
        Guid orderItemId,
        Guid pricingTierId,
        CancellationToken cancellationToken
    );
}
