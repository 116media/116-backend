using _116.Content.Application.Shared.Errors;
using _116.Content.Application.Shared.Mappers;
using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Shared.Contracts.Application.CQRS;
using MapsterMapper;

namespace _116.Content.Application.Catalog.UseCases.Admin.Commands.UpdateCategoryPricing;

/// <summary>
/// Handles the <see cref="UpdateCategoryPricingCommand" /> to update a pricing tier's price within a category.
/// </summary>
/// <param name="categoryRepository">Repository for category data access operations.</param>
/// <param name="unitOfWork">Unit of Work for managing database transactions.</param>
/// <param name="mapper">Mapster mapper for entity-to-DTO transformations.</param>
public class UpdateCategoryPricingHandler(
    ICategoryRepository categoryRepository,
    IContentUnitOfWork unitOfWork,
    IMapper mapper
) : ICommandHandler<UpdateCategoryPricingCommand, UpdateCategoryPricingResult>
{
    /// <inheritdoc />
    public async Task<UpdateCategoryPricingResult> Handle(
        UpdateCategoryPricingCommand command,
        CancellationToken cancellationToken
    )
    {
        Guid categoryId = Guid.Parse(command.CategoryId);
        Guid pricingTierId = Guid.Parse(command.PricingTierId);

        CategoryPricingEntity? pricing = await categoryRepository.GetPricingAsync(
            categoryId: categoryId,
            pricingTierId: pricingTierId,
            cancellationToken: cancellationToken
        );

        if (pricing is null)
        {
            throw CategoryErrors.PricingNotFound(categoryId: categoryId, tierId: pricingTierId);
        }

        pricing.UpdatePrice(priceUsd: command.PriceUsd);

        await unitOfWork.CommitAsync(cancellationToken: cancellationToken);

        var dto = pricing.ToCategoryPricingDto(mapper);
        return new UpdateCategoryPricingResult(Pricing: dto);
    }
}
