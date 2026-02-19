using _116.Content.Application.Shared.DTOs;
using _116.Content.Application.Shared.Errors;
using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Shared.Contracts.Application.CQRS;
using Mapster;

namespace _116.Content.Application.Lookup.UseCases.Admin.Commands.UpdatePricingTier;

/// <summary>
/// Handles the <see cref="UpdatePricingTierCommand" /> to update an existing pricing tier.
/// </summary>
/// <param name="lookupRepository">Repository for lookup data access operations.</param>
/// <param name="unitOfWork">Unit of Work for managing database transactions.</param>
public class UpdatePricingTierHandler(ILookupRepository lookupRepository, IContentUnitOfWork unitOfWork)
    : ICommandHandler<UpdatePricingTierCommand, UpdatePricingTierResult>
{
    /// <inheritdoc />
    public async Task<UpdatePricingTierResult> Handle(
        UpdatePricingTierCommand command,
        CancellationToken cancellationToken
    )
    {
        PricingTierEntity pricingTier = await lookupRepository.GetPricingTierByIdOrThrowAsync(
            id: command.Id,
            cancellationToken: cancellationToken
        );

        bool nameConflict = await lookupRepository.PricingTierExistsByNameAsync(
            name: command.Name,
            cancellationToken: cancellationToken
        );

        if (nameConflict && !string.Equals(pricingTier.Name, command.Name, StringComparison.OrdinalIgnoreCase))
        {
            throw PricingTierErrors.AlreadyExists(name: command.Name);
        }

        pricingTier.Update(name: command.Name, description: command.Description);

        await unitOfWork.CommitAsync(cancellationToken: cancellationToken);

        var dto = pricingTier.Adapt<PricingTierDto>();

        return new UpdatePricingTierResult(PricingTier: dto);
    }
}
