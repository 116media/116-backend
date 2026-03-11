using _116.Content.Application.Shared.Errors;
using _116.Content.Application.Shared.Mappers;
using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Shared.Contracts.Application.CQRS;
using MapsterMapper;

namespace _116.Content.Application.Lookup.UseCases.Admin.Commands.UpdatePricingTier;

/// <summary>
/// Handles the <see cref="AdminUpdatePricingTierCommand" /> to update an existing pricing tier.
/// </summary>
/// <param name="lookupRepository">Repository for lookup data access operations.</param>
/// <param name="unitOfWork">Unit of Work for managing database transactions.</param>
/// <param name="mapper">Mapster mapper for entity-to-DTO transformations.</param>
public class AdminUpdatePricingTierHandler(
    ILookupRepository lookupRepository,
    IContentUnitOfWork unitOfWork,
    IMapper mapper
) : ICommandHandler<AdminUpdatePricingTierCommand, AdminUpdatePricingTierResult>
{
    /// <inheritdoc />
    public async Task<AdminUpdatePricingTierResult> Handle(
        AdminUpdatePricingTierCommand command,
        CancellationToken cancellationToken
    )
    {
        Guid id = Guid.Parse(command.Id);

        PricingTierEntity pricingTier = await lookupRepository.GetPricingTierByIdOrThrowAsync(
            id: id,
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

        var dto = pricingTier.ToPricingTierDto(mapper);
        return new AdminUpdatePricingTierResult(PricingTier: dto);
    }
}
