using _116.Content.Application.Shared.Errors;
using _116.Content.Application.Shared.Mappers;
using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Shared.Contracts.Application.CQRS;
using MapsterMapper;

namespace _116.Content.Application.Lookup.UseCases.Admin.Commands.CreatePricingTier;

/// <summary>
/// Handles the <see cref="AdminCreatePricingTierCommand" /> to create a new pricing tier.
/// </summary>
/// <param name="lookupRepository">Repository for lookup data access operations.</param>
/// <param name="unitOfWork">Unit of Work for managing database transactions.</param>
/// <param name="mapper">Mapster mapper for entity-to-DTO transformations.</param>
public class AdminCreatePricingTierHandler(
    ILookupRepository lookupRepository,
    IContentUnitOfWork unitOfWork,
    IMapper mapper
) : ICommandHandler<AdminCreatePricingTierCommand, AdminCreatePricingTierResult>
{
    /// <inheritdoc />
    public async Task<AdminCreatePricingTierResult> Handle(
        AdminCreatePricingTierCommand command,
        CancellationToken cancellationToken
    )
    {
        bool exists = await lookupRepository.PricingTierExistsByNameAsync(
            name: command.Name,
            cancellationToken: cancellationToken
        );

        if (exists)
        {
            throw PricingTierErrors.AlreadyExists(name: command.Name);
        }

        var pricingTier = PricingTierEntity.Create(
            id: Guid.NewGuid(),
            name: command.Name,
            description: command.Description
        );

        await lookupRepository.AddPricingTierAsync(pricingTier: pricingTier, cancellationToken: cancellationToken);
        await unitOfWork.CommitAsync(cancellationToken: cancellationToken);

        var dto = pricingTier.ToPricingTierDto(mapper);
        return new AdminCreatePricingTierResult(PricingTier: dto);
    }
}
