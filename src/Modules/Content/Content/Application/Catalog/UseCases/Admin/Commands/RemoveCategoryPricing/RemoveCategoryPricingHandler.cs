using _116.Content.Application.Shared.DTOs;
using _116.Content.Application.Shared.Errors;
using _116.Content.Application.Shared.Mappers;
using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Shared.Contracts.Application.CQRS;
using MapsterMapper;

namespace _116.Content.Application.Catalog.UseCases.Admin.Commands.RemoveCategoryPricing;

/// <summary>
/// Handles the <see cref="RemoveCategoryPricingCommand" /> to remove a pricing tier from a category.
/// </summary>
/// <param name="categoryRepository">Repository for category data access operations.</param>
/// <param name="unitOfWork">Unit of Work for managing database transactions.</param>
/// <param name="mapper">Mapster mapper for entity-to-DTO transformations.</param>
public class RemoveCategoryPricingHandler(
    ICategoryRepository categoryRepository,
    IContentUnitOfWork unitOfWork,
    IMapper mapper
) : ICommandHandler<RemoveCategoryPricingCommand, RemoveCategoryPricingResult>
{
    /// <inheritdoc />
    public async Task<RemoveCategoryPricingResult> Handle(
        RemoveCategoryPricingCommand command,
        CancellationToken cancellationToken
    )
    {
        Guid categoryId = Guid.Parse(command.CategoryId);

        CategoryPricingEntity? pricing = await categoryRepository.GetPricingAsync(
            categoryId: categoryId,
            pricingTierId: command.PricingTierId,
            cancellationToken: cancellationToken
        );

        if (pricing is null)
        {
            throw CategoryErrors.PricingNotFound(categoryId: categoryId, tierId: command.PricingTierId);
        }

        categoryRepository.RemovePricing(pricing: pricing);
        await unitOfWork.CommitAsync(cancellationToken: cancellationToken);

        IReadOnlyList<CategoryPricingEntity> remaining = await categoryRepository.GetPricingByCategoryAsync(
            categoryId: categoryId,
            cancellationToken: cancellationToken
        );

        IReadOnlyList<CategoryPricingDto> dtoList = remaining.Select(p => p.ToCategoryPricingDto(mapper)).ToList();
        return new RemoveCategoryPricingResult(Pricing: dtoList, IsSuccess: true);
    }
}
