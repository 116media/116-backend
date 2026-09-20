using _116.Content.Application.Shared.DTOs;
using _116.Content.Application.Shared.Errors.Facade;
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
/// <param name="pricingTierRepository">Repository resolving the remaining priced tiers.</param>
/// <param name="categoryRepository">Repository for category data access operations.</param>
/// <param name="unitOfWork">Unit of Work for managing database transactions.</param>
/// <param name="mapper">Mapster mapper for entity-to-DTO transformations.</param>
/// <param name="i18n">Single i18n entry point for the Content module.</param>
public class AdminRemoveCategoryPricingHandler(
    ICategoryRepository categoryRepository,
    IPricingTierRepository pricingTierRepository,
    IContentUnitOfWork unitOfWork,
    IMapper mapper,
    ContentI18n i18n
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

        CategoryEntity category = await categoryRepository.GetByIdOrThrowAsync(
            id: categoryId,
            cancellationToken: cancellationToken
        );

        if (!category.RemovePricing(pricingTierId: pricingTierId))
        {
            throw i18n.Category.PricingNotFound(categoryId: categoryId, tierId: pricingTierId);
        }

        await unitOfWork.CommitAsync(cancellationToken: cancellationToken);

        IReadOnlyDictionary<Guid, PricingTierEntity> pricingTiers = await pricingTierRepository.GetByIdsAsync(
            ids: [.. category.Pricing.Select(pricing => pricing.PricingTierId).Distinct()],
            cancellationToken: cancellationToken
        );

        IReadOnlyList<CategoryPricingDto> dtoList =
        [
            .. category
                .Pricing.OrderBy(pricing => pricing.PricingTierId)
                .Select(pricing =>
                    pricing.ToCategoryPricingDto(mapper, pricingTiers.GetValueOrDefault(pricing.PricingTierId))
                ),
        ];

        return new AdminRemoveCategoryPricingResult(Pricing: dtoList, IsSuccess: true);
    }
}
