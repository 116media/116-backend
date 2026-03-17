using _116.Content.Application.Commerce.UseCases.Admin.Commands.AddItemTier.Contracts;
using _116.Content.Application.Shared.DTOs;
using _116.Content.Domain.Entities;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Commerce.UseCases.Admin.Commands.AddItemTier;

/// <summary>
/// Handles the <see cref="AdminAddItemTierCommand" /> to attach a pricing tier snapshot to a specific order item.
/// </summary>
/// <param name="addItemTierFactory">Factory for the tier attachment flow.</param>
public class AdminAddItemTierHandler(IAddItemTierFactory addItemTierFactory)
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

        (ContentItemTierEntity tier, string tierName) = await addItemTierFactory.AttachTierAsync(
            orderId: orderId,
            orderItemId: itemId,
            pricingTierId: tierId,
            cancellationToken: cancellationToken
        );

        var dto = new ItemTierDto(TierName: tierName, PriceSnapshotUsd: tier.PriceSnapshotUsd);
        return new AdminAddItemTierResult(Tier: dto);
    }
}
