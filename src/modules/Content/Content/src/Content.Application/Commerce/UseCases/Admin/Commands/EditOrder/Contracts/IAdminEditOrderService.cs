using _116.Content.Domain.Entities;

namespace _116.Content.Application.Commerce.UseCases.Admin.Commands.EditOrder.Contracts;

/// <summary>
/// Resolves and applies an order edit: loads the order, validates the new customer and updates
/// through the order aggregate.
/// </summary>
public interface IAdminEditOrderService
{
    /// <summary>
    /// Applies the edit, throwing the localized error when the order or the customer does not
    /// exist. The caller owns the commit.
    /// </summary>
    /// <param name="orderId">The order being edited.</param>
    /// <param name="customerId">The new customer, or null to keep the current one.</param>
    /// <param name="packageId">The new package, or null to keep the current one.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>The edited order.</returns>
    Task<ContentOrderEntity> EditAsync(
        Guid orderId,
        Guid? customerId,
        Guid? packageId,
        CancellationToken cancellationToken
    );
}
