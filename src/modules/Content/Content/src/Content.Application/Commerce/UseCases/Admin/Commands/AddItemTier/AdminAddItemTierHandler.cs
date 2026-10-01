using _116.BuildingBlocks.Application.CQRS;
using _116.Content.Application.Commerce.UseCases.Admin.Commands.AddItemTier.Contracts;
using _116.Content.Application.Shared.DTOs;
using _116.Content.Domain.Entities;

namespace _116.Content.Application.Commerce.UseCases.Admin.Commands.AddItemTier;

/// <summary>
/// Handles the <see cref="AdminAddItemTierCommand" /> to attach a pricing tier snapshot to a specific order item.
/// </summary>
/// <param name="addItemTierService">Service for the tier attachment flow.</param>
public class AdminAddItemTierHandler(IAddItemTierService addItemTierService)
    : ICommandHandler<AdminAddItemTierCommand, AdminAddItemTierResult>
{
    /// <inheritdoc />
    public async Task<AdminAddItemTierResult> Handle(
        AdminAddItemTierCommand command,
        CancellationToken cancellationToken
    )
    {
        Guid orderId = Guid.Parse(command.OrderId);
        Guid itemId = Guid.Parse(command.OrderItemId);
        Guid tierId = Guid.Parse(command.PricingTierId);

        (ContentItemTierEntity tier, string tierName) = await addItemTierService.AttachTierAsync(
            orderId: orderId,
            orderItemId: itemId,
            pricingTierId: tierId,
            cancellationToken: cancellationToken
        );

        var dto = new ItemTierDto(Id: tier.Id, TierName: tierName, PriceSnapshotUsd: tier.PriceSnapshotUsd);
        return new AdminAddItemTierResult(Tier: dto);
    }
}
