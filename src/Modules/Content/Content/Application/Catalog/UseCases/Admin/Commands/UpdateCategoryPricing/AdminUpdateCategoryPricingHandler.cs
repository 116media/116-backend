using _116.Content.Application.Shared.Errors.Facade;
using _116.Content.Application.Shared.Mappers;
using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Shared.Contracts.Application.CQRS;
using MapsterMapper;

namespace _116.Content.Application.Catalog.UseCases.Admin.Commands.UpdateCategoryPricing;

/// <summary>
/// Handles the <see cref="AdminUpdateCategoryPricingCommand" /> to update a pricing tier's price within a category.
/// </summary>
/// <param name="categoryRepository">Repository for category data access operations.</param>
/// <param name="unitOfWork">Unit of Work for managing database transactions.</param>
/// <param name="mapper">Mapster mapper for entity-to-DTO transformations.</param>
/// <param name="i18n">Single i18n entry point for the Content module.</param>
public class AdminUpdateCategoryPricingHandler(
    ICategoryRepository categoryRepository,
    IContentUnitOfWork unitOfWork,
    IMapper mapper,
    ContentI18n i18n
) : ICommandHandler<AdminUpdateCategoryPricingCommand, AdminUpdateCategoryPricingResult>
{
    /// <inheritdoc />
    public async Task<AdminUpdateCategoryPricingResult> Handle(
        AdminUpdateCategoryPricingCommand command,
        CancellationToken cancellationToken
    )
    {
        Guid categoryId = Guid.Parse(command.CategoryId);
        Guid pricingTierId = Guid.Parse(command.PricingTierId);

        await categoryRepository.GetByIdOrThrowAsync(id: categoryId, cancellationToken: cancellationToken);

        CategoryPricingEntity? pricing = await categoryRepository.GetPricingAsync(
            categoryId: categoryId,
            pricingTierId: pricingTierId,
            cancellationToken: cancellationToken
        );

        if (pricing is not null)
        {
            pricing.UpdatePrice(priceUsd: command.PriceUsd);

            await unitOfWork.CommitAsync(cancellationToken: cancellationToken);

            var dto = pricing.ToCategoryPricingDto(mapper);
            return new AdminUpdateCategoryPricingResult(Pricing: dto);
        }

        throw i18n.Category.PricingNotFound(categoryId: categoryId, tierId: pricingTierId);
    }
}
