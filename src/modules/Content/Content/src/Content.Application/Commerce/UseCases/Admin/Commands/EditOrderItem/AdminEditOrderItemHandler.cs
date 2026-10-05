using _116.BuildingBlocks.Application.CQRS;
using _116.Content.Application.Commerce.Services;
using _116.Content.Application.Commerce.UseCases.Admin.Commands.EditOrderItem.Contracts;
using _116.Content.Application.Shared.DTOs;
using _116.Content.Application.Shared.Persistence;
using _116.Content.Domain.Entities;

namespace _116.Content.Application.Commerce.UseCases.Admin.Commands.EditOrderItem;

/// <summary>
/// Handles the <see cref="AdminEditOrderItemCommand" /> to change an item of a draft order.
/// </summary>
/// <param name="editItemService">Service resolving and applying the edit.</param>
/// <param name="unitOfWork">Unit of Work for managing database transactions.</param>
/// <param name="orderDtoService">Service assembling the item DTO.</param>
public class AdminEditOrderItemHandler(
    IAdminEditOrderItemService editItemService,
    IContentUnitOfWork unitOfWork,
    IContentOrderDtoService orderDtoService
) : ICommandHandler<AdminEditOrderItemCommand, AdminEditOrderItemResult>
{
    /// <inheritdoc />
    public async Task<AdminEditOrderItemResult> Handle(
        AdminEditOrderItemCommand command,
        CancellationToken cancellationToken
    )
    {
        ContentOrderItemEntity item = await editItemService.EditAsync(command, cancellationToken);
        await unitOfWork.CommitAsync(cancellationToken: cancellationToken);

        OrderItemDto dto = await orderDtoService.CreateItemAsync(item, cancellationToken);
        return new AdminEditOrderItemResult(Item: dto);
    }
}
