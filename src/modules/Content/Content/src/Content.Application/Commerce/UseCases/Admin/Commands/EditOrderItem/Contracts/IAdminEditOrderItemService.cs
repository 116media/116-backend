using _116.Content.Domain.Entities;

namespace _116.Content.Application.Commerce.UseCases.Admin.Commands.EditOrderItem.Contracts;

/// <summary>
/// Resolves and applies an order item edit: loads the draft order and the item, validates the
/// new category and promotion level, and updates through the order aggregate.
/// </summary>
public interface IAdminEditOrderItemService
{
    /// <summary>
    /// Applies the edit, throwing the localized error when the order or item is missing or an
    /// association is not commissionable. The caller owns the commit.
    /// </summary>
    /// <param name="command">The edit command.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>The edited item.</returns>
    Task<ContentOrderItemEntity> EditAsync(AdminEditOrderItemCommand command, CancellationToken cancellationToken);
}
