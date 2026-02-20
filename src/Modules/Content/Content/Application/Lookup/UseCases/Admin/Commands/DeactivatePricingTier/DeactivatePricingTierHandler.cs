using _116.Content.Application.Shared.Errors;
using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Lookup.UseCases.Admin.Commands.DeactivatePricingTier;

/// <summary>
/// Handles the <see cref="DeactivatePricingTierCommand" /> to deactivate a pricing tier.
/// </summary>
/// <param name="lookupRepository">Repository for lookup data access operations.</param>
/// <param name="unitOfWork">Unit of Work for managing database transactions.</param>
public class DeactivatePricingTierHandler(ILookupRepository lookupRepository, IContentUnitOfWork unitOfWork)
    : ICommandHandler<DeactivatePricingTierCommand>
{
    /// <inheritdoc />
    public async Task Handle(DeactivatePricingTierCommand command, CancellationToken cancellationToken)
    {
        PricingTierEntity pricingTier = await lookupRepository.GetPricingTierByIdOrThrowAsync(
            id: command.Id,
            cancellationToken: cancellationToken
        );

        bool deactivated = pricingTier.Deactivate();

        if (!deactivated)
        {
            throw PricingTierErrors.AlreadyInactive();
        }

        await unitOfWork.CommitAsync(cancellationToken: cancellationToken);
    }
}
