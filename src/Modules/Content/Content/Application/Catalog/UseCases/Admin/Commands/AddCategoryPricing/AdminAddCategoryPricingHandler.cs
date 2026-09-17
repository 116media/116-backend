using _116.Content.Application.Shared.DTOs;
using _116.Content.Application.Shared.Errors.Facade;
using _116.Content.Application.Shared.Mappers;
using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Shared.Contracts.Application.CQRS;
using MapsterMapper;

namespace _116.Content.Application.Catalog.UseCases.Admin.Commands.AddCategoryPricing;

/// <summary>
/// Handles the <see cref="AdminAddCategoryPricingCommand" /> to attach a pricing tier to a category.
/// </summary>
/// <param name="categoryRepository">Repository for category data access operations.</param>
/// <param name="pricingTierRepository">Repository for verifying pricing tier existence and active status.</param>
/// <param name="unitOfWork">Unit of Work for managing database transactions.</param>
/// <param name="mapper">Mapster mapper for entity-to-DTO transformations.</param>
/// <param name="i18n">Single i18n entry point for the Content module.</param>
public class AdminAddCategoryPricingHandler(
    ICategoryRepository categoryRepository,
    IPricingTierRepository pricingTierRepository,
    IContentUnitOfWork unitOfWork,
    IMapper mapper,
    ContentI18n i18n
) : ICommandHandler<AdminAddCategoryPricingCommand, AdminAddCategoryPricingResult>
{
    /// <inheritdoc />
    public async Task<AdminAddCategoryPricingResult> Handle(
        AdminAddCategoryPricingCommand command,
        CancellationToken cancellationToken
    )
    {
        Guid categoryId = Guid.Parse(command.CategoryId);

        CategoryEntity category = await categoryRepository.GetByIdOrThrowAsync(
            id: categoryId,
            cancellationToken: cancellationToken
        );

        PricingTierEntity pricingTier = await pricingTierRepository.GetByIdOrThrowAsync(
            id: command.PricingTierId,
            cancellationToken: cancellationToken
        );

        if (!pricingTier.IsActive)
        {
            throw i18n.PricingTier.IsInactive();
        }

        if (category.FindPricing(pricingTierId: command.PricingTierId) is not null)
        {
            throw i18n.Category.PricingAlreadyExists();
        }

        category.SetPricing(pricingTierId: command.PricingTierId, priceUsd: command.PriceUsd);
        await unitOfWork.CommitAsync(cancellationToken: cancellationToken);

        CategoryPricingEntity created = category.FindPricing(pricingTierId: command.PricingTierId)!;

        CategoryPricingDto dto = created.ToCategoryPricingDto(mapper, pricingTier);
        return new AdminAddCategoryPricingResult(Pricing: dto);
    }
}
