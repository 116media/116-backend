using _116.BuildingBlocks.Application.CQRS;
using _116.Content.Application.Commerce.Services;
using _116.Content.Application.Commerce.UseCases.Admin.Commands.EditOrder.Contracts;
using _116.Content.Application.Shared.DTOs;
using _116.Content.Application.Shared.Persistence;
using _116.Content.Domain.Entities;

namespace _116.Content.Application.Commerce.UseCases.Admin.Commands.EditOrder;

/// <summary>
/// Handles the <see cref="AdminEditOrderCommand" /> to change an order's customer or package.
/// </summary>
/// <param name="editOrderService">Service resolving and applying the edit.</param>
/// <param name="unitOfWork">Unit of Work for managing database transactions.</param>
/// <param name="orderDtoService">Service assembling the order summary.</param>
public class AdminEditOrderHandler(
    IAdminEditOrderService editOrderService,
    IContentUnitOfWork unitOfWork,
    IContentOrderDtoService orderDtoService
) : ICommandHandler<AdminEditOrderCommand, AdminEditOrderResult>
{
    /// <inheritdoc />
    public async Task<AdminEditOrderResult> Handle(AdminEditOrderCommand command, CancellationToken cancellationToken)
    {
        ContentOrderEntity order = await editOrderService.EditAsync(
            orderId: Guid.Parse(command.OrderId),
            customerId: command.CustomerId is not null ? Guid.Parse(command.CustomerId) : null,
            packageId: command.PackageId,
            cancellationToken: cancellationToken
        );

        await unitOfWork.CommitAsync(cancellationToken: cancellationToken);

        ContentOrderSummaryDto dto = await orderDtoService.CreateSummaryAsync(order, cancellationToken);
        return new AdminEditOrderResult(Order: dto);
    }
}
