using _116.Content.Application.Shared.DTOs;
using _116.Content.Application.Shared.Errors;
using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Shared.Contracts.Application.CQRS;
using Mapster;

namespace _116.Content.Application.Lookup.UseCases.Admin.Commands.CreatePricingTier;

/// <summary>
/// Handles the <see cref="CreatePricingTierCommand" /> to create a new pricing tier.
/// </summary>
/// <param name="lookupRepository">Repository for lookup data access operations.</param>
/// <param name="unitOfWork">Unit of Work for managing database transactions.</param>
public class CreatePricingTierHandler(ILookupRepository lookupRepository, IContentUnitOfWork unitOfWork)
    : ICommandHandler<CreatePricingTierCommand, CreatePricingTierResult>
{
    /// <inheritdoc />
    public async Task<CreatePricingTierResult> Handle(
        CreatePricingTierCommand command,
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

        var dto = pricingTier.Adapt<PricingTierDto>();

        return new CreatePricingTierResult(PricingTier: dto);
    }
}
