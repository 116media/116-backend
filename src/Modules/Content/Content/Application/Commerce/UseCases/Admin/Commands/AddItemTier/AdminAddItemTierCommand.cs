using _116.Content.Application.Shared.DTOs;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Commerce.UseCases.Admin.Commands.AddItemTier;

/// <summary>
/// Command for attaching a pricing tier snapshot to a specific order item.
/// </summary>
/// <param name="OrderId">The identifier of the order.</param>
/// <param name="OrderItemId">The identifier of the order item to attach the tier to.</param>
/// <param name="PricingTierId">The pricing tier to snapshot.</param>
public record AdminAddItemTierCommand(string OrderId, string OrderItemId, string PricingTierId)
    : ICommand<AdminAddItemTierResult>;

/// <summary>
/// Result of the <see cref="AdminAddItemTierCommand" /> containing the created tier snapshot.
/// </summary>
/// <param name="Tier">The created tier snapshot.</param>
public record AdminAddItemTierResult(ItemTierDto Tier);
