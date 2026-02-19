using _116.Content.Application.Shared.Errors;
using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Lookup.UseCases.Admin.Commands.ActivatePricingTier;

/// <summary>
/// Handles the <see cref="ActivatePricingTierCommand" /> to activate a pricing tier.
/// </summary>
/// <param name="lookupRepository">Repository for lookup data access operations.</param>
/// <param name="unitOfWork">Unit of Work for managing database transactions.</param>
public class ActivatePricingTierHandler(ILookupRepository lookupRepository, IContentUnitOfWork unitOfWork)
    : ICommandHandler<ActivatePricingTierCommand>
{
    /// <inheritdoc />
    public async Task Handle(ActivatePricingTierCommand command, CancellationToken cancellationToken)
    {
        PricingTierEntity pricingTier = await lookupRepository.GetPricingTierByIdOrThrowAsync(
            id: command.Id,
            cancellationToken: cancellationToken
        );

        bool activated = pricingTier.Activate();

        if (!activated)
        {
            throw PricingTierErrors.AlreadyActive();
        }

        await unitOfWork.CommitAsync(cancellationToken: cancellationToken);
    }
}
