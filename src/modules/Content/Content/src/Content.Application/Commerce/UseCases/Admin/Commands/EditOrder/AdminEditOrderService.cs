using _116.Content.Application.Commerce.UseCases.Admin.Commands.EditOrder.Contracts;
using _116.Content.Application.Shared.Errors.Facade;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;

namespace _116.Content.Application.Commerce.UseCases.Admin.Commands.EditOrder;

/// <summary>
/// Resolves and applies the edit for the admin edit-order use case.
/// </summary>
/// <param name="contentOrderRepository">Repository loading the order with its items.</param>
/// <param name="customerRepository">Repository validating the new customer.</param>
/// <param name="i18n">Single i18n entry point for the Content module.</param>
public class AdminEditOrderService(
    IContentOrderRepository contentOrderRepository,
    ICustomerRepository customerRepository,
    ContentI18n i18n
) : IAdminEditOrderService
{
    /// <inheritdoc />
    public async Task<ContentOrderEntity> EditAsync(
        Guid orderId,
        Guid? customerId,
        Guid? packageId,
        CancellationToken cancellationToken
    )
    {
        ContentOrderEntity? order = await contentOrderRepository.GetByIdWithItemsAsync(
            id: orderId,
            ct: cancellationToken
        );

        if (order is null)
        {
            throw i18n.ContentOrder.NotFound(id: orderId);
        }

        if (customerId is { } newCustomerId)
        {
            await customerRepository.GetByIdOrThrowAsync(id: newCustomerId, cancellationToken: cancellationToken);
        }

        order.Update(customerId: customerId, packageId: packageId);

        return order;
    }
}
