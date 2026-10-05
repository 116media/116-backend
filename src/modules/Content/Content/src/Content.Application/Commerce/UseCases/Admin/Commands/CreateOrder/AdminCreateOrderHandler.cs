using _116.BuildingBlocks.Application.CQRS;
using _116.Content.Application.Commerce.UseCases.Admin.Commands.CreateOrder.Contracts;
using _116.Content.Application.Shared.DTOs;
using _116.Content.Application.Shared.Persistence;

namespace _116.Content.Application.Commerce.UseCases.Admin.Commands.CreateOrder;

/// <summary>
/// Handles the <see cref="AdminCreateOrderCommand" /> to open an order for a customer, seeded
/// from a package when one is named.
/// </summary>
/// <param name="createOrderService">Service resolving the customer and package and staging the order.</param>
/// <param name="unitOfWork">Unit of Work for managing database transactions.</param>
public class AdminCreateOrderHandler(ICreateOrderService createOrderService, IContentUnitOfWork unitOfWork)
    : ICommandHandler<AdminCreateOrderCommand, AdminCreateOrderResult>
{
    /// <inheritdoc />
    public async Task<AdminCreateOrderResult> Handle(
        AdminCreateOrderCommand command,
        CancellationToken cancellationToken
    )
    {
        CreatedOrderData created = await createOrderService.CreateAsync(
            customerId: Guid.Parse(command.CustomerId),
            packageId: command.PackageId,
            ct: cancellationToken
        );

        await unitOfWork.CommitAsync(cancellationToken: cancellationToken);

        var dto = new ContentOrderSummaryDto(
            Id: created.Order.Id,
            Status: created.Order.Status,
            CustomerName: created.Customer.FullName,
            TotalAmountUsd: created.Order.TotalAmountUsd,
            ItemCount: created.ItemCount
        );
        return new AdminCreateOrderResult(Order: dto);
    }
}
