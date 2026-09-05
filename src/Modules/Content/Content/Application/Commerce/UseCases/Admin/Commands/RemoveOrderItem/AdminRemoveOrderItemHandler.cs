using _116.Content.Application.Shared.Errors.Facade;
using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Commerce.UseCases.Admin.Commands.RemoveOrderItem;

/// <summary>
/// Handles the <see cref="AdminRemoveOrderItemCommand" /> to remove a content item from a draft order.
/// </summary>
/// <param name="contentOrderRepository">Repository for content order data access operations.</param>
/// <param name="unitOfWork">Unit of Work for managing database transactions.</param>
/// <param name="i18n">Single i18n entry point for the Content module.</param>
public class AdminRemoveOrderItemHandler(
    IContentOrderRepository contentOrderRepository,
    IContentUnitOfWork unitOfWork,
    ContentI18n i18n
) : ICommandHandler<AdminRemoveOrderItemCommand, AdminRemoveOrderItemResult>
{
    /// <inheritdoc />
    public async Task<AdminRemoveOrderItemResult> Handle(
        AdminRemoveOrderItemCommand command,
        CancellationToken cancellationToken
    )
    {
        Guid orderId = Guid.Parse(command.OrderId);
        Guid itemId = Guid.Parse(command.ItemId);

        ContentOrderEntity order =
            await contentOrderRepository.GetByIdWithItemsAsync(id: orderId, ct: cancellationToken)
            ?? throw i18n.ContentOrder.NotFound(id: orderId);

        order.EnsureDraft(i18n.ContentOrder);

        ContentOrderItemEntity item = await contentOrderRepository.GetItemByIdOrThrowAsync(
            orderId: orderId,
            itemId: itemId,
            ct: cancellationToken
        );

        // Same scope, so this is the tracked instance inside order.Items; the navigation removal
        // recalculates the total and the repository call deletes the row, in one transaction.
        order.RemoveItem(item);
        await contentOrderRepository.RemoveItemAsync(item: item, ct: cancellationToken);

        await contentOrderRepository.UpdateAsync(order: order, ct: cancellationToken);
        await unitOfWork.CommitAsync(cancellationToken: cancellationToken);

        return new AdminRemoveOrderItemResult(IsSuccess: true);
    }
}
