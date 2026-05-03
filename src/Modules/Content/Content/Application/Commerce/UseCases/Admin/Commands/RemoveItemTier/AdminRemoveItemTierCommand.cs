using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Commerce.UseCases.Admin.Commands.RemoveItemTier;

/// <summary>
/// Command for removing a pricing tier from an order item in a draft order.
/// </summary>
/// <param name="OrderId">The identifier of the order.</param>
/// <param name="ItemId">The identifier of the order item.</param>
/// <param name="TierId">The identifier of the pricing tier to remove.</param>
public record AdminRemoveItemTierCommand(string OrderId, string ItemId, string TierId)
    : ICommand<AdminRemoveItemTierResult>;

/// <summary>
/// Result of the <see cref="AdminRemoveItemTierCommand" />.
/// </summary>
/// <param name="IsSuccess">Indicates whether the tier was successfully removed.</param>
public record AdminRemoveItemTierResult(bool IsSuccess);
