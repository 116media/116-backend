using _116.Content.Application.Shared.Errors;
using _116.Content.Application.Shared.Mappers;
using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Shared.Contracts.Application.CQRS;
using MapsterMapper;

namespace _116.Content.Application.Lookup.UseCases.Admin.Commands.DeactivatePricingTier;

/// <summary>
/// Handles the <see cref="AdminDeactivatePricingTierCommand" /> to deactivate a pricing tier.
/// </summary>
/// <param name="lookupRepository">Repository for lookup data access operations.</param>
/// <param name="unitOfWork">Unit of Work for managing database transactions.</param>
/// <param name="mapper">Mapster mapper for entity-to-DTO transformations.</param>
public class AdminDeactivatePricingTierHandler(
    ILookupRepository lookupRepository,
    IContentUnitOfWork unitOfWork,
    IMapper mapper
) : ICommandHandler<AdminDeactivatePricingTierCommand, AdminDeactivatePricingTierResult>
{
    /// <inheritdoc />
    public async Task<AdminDeactivatePricingTierResult> Handle(
        AdminDeactivatePricingTierCommand command,
        CancellationToken cancellationToken
    )
    {
        Guid id = Guid.Parse(command.Id);

        PricingTierEntity pricingTier = await lookupRepository.GetPricingTierByIdOrThrowAsync(
            id: id,
            cancellationToken: cancellationToken
        );

        bool deactivated = pricingTier.Deactivate();

        if (!deactivated)
        {
            throw PricingTierErrors.AlreadyInactive();
        }

        await unitOfWork.CommitAsync(cancellationToken: cancellationToken);

        var dto = pricingTier.ToPricingTierDto(mapper);
        return new AdminDeactivatePricingTierResult(PricingTier: dto);
    }
}
