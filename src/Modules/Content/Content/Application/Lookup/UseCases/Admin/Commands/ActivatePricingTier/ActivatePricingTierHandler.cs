using _116.Content.Application.Shared.Errors;
using _116.Content.Application.Shared.Mappers;
using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Shared.Contracts.Application.CQRS;
using MapsterMapper;

namespace _116.Content.Application.Lookup.UseCases.Admin.Commands.ActivatePricingTier;

/// <summary>
/// Handles the <see cref="ActivatePricingTierCommand" /> to activate a pricing tier.
/// </summary>
/// <param name="lookupRepository">Repository for lookup data access operations.</param>
/// <param name="unitOfWork">Unit of Work for managing database transactions.</param>
/// <param name="mapper">Mapster mapper for entity-to-DTO transformations.</param>
public class ActivatePricingTierHandler(
    ILookupRepository lookupRepository,
    IContentUnitOfWork unitOfWork,
    IMapper mapper
) : ICommandHandler<ActivatePricingTierCommand, ActivatePricingTierResult>
{
    /// <inheritdoc />
    public async Task<ActivatePricingTierResult> Handle(
        ActivatePricingTierCommand command,
        CancellationToken cancellationToken
    )
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

        var dto = pricingTier.ToPricingTierDto(mapper);
        return new ActivatePricingTierResult(PricingTier: dto);
    }
}
