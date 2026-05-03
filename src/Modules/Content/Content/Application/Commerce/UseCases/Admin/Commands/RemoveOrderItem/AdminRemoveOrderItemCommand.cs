using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Commerce.UseCases.Admin.Commands.RemoveOrderItem;

/// <summary>
/// Command for removing a content item from a draft order.
/// </summary>
/// <param name="OrderId">The identifier of the order containing the item.</param>
/// <param name="ItemId">The identifier of the item to remove.</param>
public record AdminRemoveOrderItemCommand(string OrderId, string ItemId) : ICommand<AdminRemoveOrderItemResult>;

/// <summary>
/// Result of the <see cref="AdminRemoveOrderItemCommand" />.
/// </summary>
/// <param name="IsSuccess">Indicates whether the item was successfully removed.</param>
public record AdminRemoveOrderItemResult(bool IsSuccess);
