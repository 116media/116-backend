using _116.BuildingBlocks.Application.CQRS;
using _116.Content.Application.Catalog.UseCases.Admin.Commands.AddCategoryPricing.Contracts;
using _116.Content.Application.Shared.DTOs;
using _116.Content.Application.Shared.Mappers;
using _116.Content.Application.Shared.Persistence;
using MapsterMapper;

namespace _116.Content.Application.Catalog.UseCases.Admin.Commands.AddCategoryPricing;

/// <summary>
/// Handles the <see cref="AdminAddCategoryPricingCommand" /> to price a category for a tier.
/// </summary>
/// <param name="addPricingService">Service resolving and applying the pricing row.</param>
/// <param name="unitOfWork">Unit of Work for managing database transactions.</param>
/// <param name="mapper">Mapster mapper for entity-to-DTO transformations.</param>
public class AdminAddCategoryPricingHandler(
    IAdminAddCategoryPricingService addPricingService,
    IContentUnitOfWork unitOfWork,
    IMapper mapper
) : ICommandHandler<AdminAddCategoryPricingCommand, AdminAddCategoryPricingResult>
{
    /// <inheritdoc />
    public async Task<AdminAddCategoryPricingResult> Handle(
        AdminAddCategoryPricingCommand command,
        CancellationToken cancellationToken
    )
    {
        CategoryPricingData added = await addPricingService.AddAsync(
            categoryId: Guid.Parse(command.CategoryId),
            pricingTierId: command.PricingTierId,
            priceUsd: command.PriceUsd,
            cancellationToken: cancellationToken
        );

        await unitOfWork.CommitAsync(cancellationToken: cancellationToken);

        CategoryPricingDto dto = added.Pricing.ToCategoryPricingDto(mapper, added.Tier);
        return new AdminAddCategoryPricingResult(Pricing: dto);
    }
}
