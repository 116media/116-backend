using _116.Content.Application.Catalog.Factories;
using _116.Content.Application.Shared.DTOs;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Catalog.UseCases.Public.Queries.GetActiveCategories;

/// <summary>
/// Handles the <see cref="PublicGetActiveCategoriesQuery" /> to retrieve the list of active public categories.
/// </summary>
/// <param name="categoryRepository">Repository for category data access operations.</param>
/// <param name="categoryDtoFactory">Builds category projections with their posters resolved.</param>
public class PublicGetActiveCategoriesHandler(
    ICategoryRepository categoryRepository,
    ICategoryDtoFactory categoryDtoFactory
) : IQueryHandler<PublicGetActiveCategoriesQuery, PublicGetActiveCategoriesResult>
{
    /// <inheritdoc />
    public async Task<PublicGetActiveCategoriesResult> Handle(
        PublicGetActiveCategoriesQuery query,
        CancellationToken cancellationToken
    )
    {
        IReadOnlyList<CategoryEntity> categories = await categoryRepository.GetActiveByContentTypeAsync(
            contentTypeId: query.ContentTypeId,
            cancellationToken: cancellationToken
        );

        IReadOnlyList<CategoryDto> dtoList = await categoryDtoFactory.CreateManyAsync(categories, cancellationToken);

        return new PublicGetActiveCategoriesResult(Categories: dtoList);
    }
}
