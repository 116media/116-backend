using _116.Content.Application.Shared.Errors;
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
/// <param name="lookupRepository">Repository for verifying pricing tier existence and active status.</param>
/// <param name="unitOfWork">Unit of Work for managing database transactions.</param>
/// <param name="mapper">Mapster mapper for entity-to-DTO transformations.</param>
public class AdminAddCategoryPricingHandler(
    ICategoryRepository categoryRepository,
    ILookupRepository lookupRepository,
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
        Guid categoryId = Guid.Parse(command.CategoryId);

        await categoryRepository.GetByIdOrThrowAsync(id: categoryId, cancellationToken: cancellationToken);

        PricingTierEntity pricingTier = await lookupRepository.GetPricingTierByIdOrThrowAsync(
            id: command.PricingTierId,
            cancellationToken: cancellationToken
        );

        if (!pricingTier.IsActive)
        {
            throw PricingTierErrors.IsInactive();
        }

        CategoryPricingEntity? existing = await categoryRepository.GetPricingAsync(
            categoryId: categoryId,
            pricingTierId: command.PricingTierId,
            cancellationToken: cancellationToken
        );

        if (existing is not null)
        {
            throw CategoryErrors.PricingAlreadyExists();
        }

        var pricing = CategoryPricingEntity.Create(
            id: Guid.NewGuid(),
            categoryId: categoryId,
            pricingTierId: command.PricingTierId,
            priceUsd: command.PriceUsd
        );

        await categoryRepository.AddPricingAsync(pricing: pricing, cancellationToken: cancellationToken);
        await unitOfWork.CommitAsync(cancellationToken: cancellationToken);

        CategoryPricingEntity created =
            await categoryRepository.GetPricingAsync(
                categoryId: categoryId,
                pricingTierId: command.PricingTierId,
                cancellationToken: cancellationToken
            ) ?? pricing;

        var dto = created.ToCategoryPricingDto(mapper);
        return new AdminAddCategoryPricingResult(Pricing: dto);
    }
}
