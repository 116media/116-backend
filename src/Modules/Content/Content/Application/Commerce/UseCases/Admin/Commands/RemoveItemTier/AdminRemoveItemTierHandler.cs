using _116.Content.Application.Shared.Errors.Facade;
using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Commerce.UseCases.Admin.Commands.RemoveItemTier;

/// <summary>
/// Handles the <see cref="AdminRemoveItemTierCommand" /> to remove a pricing tier from an order item.
/// </summary>
/// <param name="contentOrderRepository">Repository for content order data access operations.</param>
/// <param name="unitOfWork">Unit of Work for managing database transactions.</param>
/// <param name="i18n">Single i18n entry point for the Content module.</param>
public class AdminRemoveItemTierHandler(
    IContentOrderRepository contentOrderRepository,
    IContentUnitOfWork unitOfWork,
    ContentI18n i18n
) : ICommandHandler<AdminRemoveItemTierCommand, AdminRemoveItemTierResult>
{
    /// <inheritdoc />
    public async Task<AdminRemoveItemTierResult> Handle(
        AdminRemoveItemTierCommand command,
        CancellationToken cancellationToken
    )
    {
        Guid orderId = Guid.Parse(command.OrderId);
        Guid itemId = Guid.Parse(command.ItemId);
        Guid tierId = Guid.Parse(command.TierId);

        ContentOrderEntity order = await contentOrderRepository.GetByIdOrThrowAsync(id: orderId, ct: cancellationToken);

        order.EnsureDraft();

        ContentOrderItemEntity? item = order.FindItem(itemId: itemId);

        if (item is null)
        {
            throw i18n.ContentOrder.ItemNotFound(itemId: itemId);
        }

        if (!order.RemoveTier(item: item, tierId: tierId))
        {
            throw i18n.ContentOrder.ItemTierNotFound(tierId: tierId);
        }

        await unitOfWork.CommitAsync(cancellationToken: cancellationToken);

        return new AdminRemoveItemTierResult(IsSuccess: true);
    }
}
