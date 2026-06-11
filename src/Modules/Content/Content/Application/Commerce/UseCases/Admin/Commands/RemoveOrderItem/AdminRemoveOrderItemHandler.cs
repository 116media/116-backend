using _116.Content.Application.Shared.Errors;
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
/// <param name="contentOrderErrors">Content order domain error factory.</param>
public class AdminRemoveOrderItemHandler(
    IContentOrderRepository contentOrderRepository,
    IContentUnitOfWork unitOfWork,
    ContentOrderErrors contentOrderErrors
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
            ?? throw contentOrderErrors.NotFound(id: orderId);

        order.EnsureDraft(contentOrderErrors);

        ContentOrderItemEntity item = await contentOrderRepository.GetItemByIdOrThrowAsync(
            orderId: orderId,
            itemId: itemId,
            ct: cancellationToken
        );

        await contentOrderRepository.RemoveItemAsync(item: item, ct: cancellationToken);
        await unitOfWork.CommitAsync(cancellationToken: cancellationToken);

        ContentOrderEntity updated =
            await contentOrderRepository.GetByIdWithItemsAsync(id: orderId, ct: cancellationToken)
            ?? throw contentOrderErrors.NotFound(id: orderId);

        updated.RecalculateTotalFromItems();
        await contentOrderRepository.UpdateAsync(order: updated, ct: cancellationToken);
        await unitOfWork.CommitAsync(cancellationToken: cancellationToken);

        return new AdminRemoveOrderItemResult(IsSuccess: true);
    }
}
