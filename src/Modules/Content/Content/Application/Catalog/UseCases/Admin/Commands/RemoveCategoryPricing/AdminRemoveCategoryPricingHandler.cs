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
/// Handles the <see cref="AdminRemoveCategoryPricingCommand" /> to remove a pricing tier from a category.
/// </summary>
/// <param name="categoryRepository">Repository for category data access operations.</param>
/// <param name="unitOfWork">Unit of Work for managing database transactions.</param>
/// <param name="mapper">Mapster mapper for entity-to-DTO transformations.</param>
public class AdminRemoveCategoryPricingHandler(
    ICategoryRepository categoryRepository,
    IContentUnitOfWork unitOfWork,
    IMapper mapper
) : ICommandHandler<AdminRemoveCategoryPricingCommand, AdminRemoveCategoryPricingResult>
{
    /// <inheritdoc />
    public async Task<AdminRemoveCategoryPricingResult> Handle(
        AdminRemoveCategoryPricingCommand command,
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

        categoryRepository.RemovePricing(pricing: pricing);
        await unitOfWork.CommitAsync(cancellationToken: cancellationToken);

        IReadOnlyList<CategoryPricingEntity> remaining = await categoryRepository.GetPricingByCategoryAsync(
            categoryId: categoryId,
            cancellationToken: cancellationToken
        );

        IReadOnlyList<CategoryPricingDto> dtoList = remaining.Select(p => p.ToCategoryPricingDto(mapper)).ToList();
        return new AdminRemoveCategoryPricingResult(Pricing: dtoList, IsSuccess: true);
    }
}
