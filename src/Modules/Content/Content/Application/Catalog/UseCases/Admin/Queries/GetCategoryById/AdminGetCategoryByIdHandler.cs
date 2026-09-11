using _116.Content.Application.Catalog.Factories;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Catalog.UseCases.Admin.Queries.GetCategoryById;

/// <summary>
/// Handles the <see cref="AdminGetCategoryByIdQuery" /> to retrieve a category by its identifier.
/// </summary>
/// <param name="categoryRepository">Repository for category data access operations.</param>
/// <param name="categoryDtoFactory">Builds category projections with their posters resolved.</param>
public class AdminGetCategoryByIdHandler(ICategoryRepository categoryRepository, ICategoryDtoFactory categoryDtoFactory)
    : IQueryHandler<AdminGetCategoryByIdQuery, AdminGetCategoryByIdResult>
{
    /// <inheritdoc />
    public async Task<AdminGetCategoryByIdResult> Handle(
        AdminGetCategoryByIdQuery query,
        CancellationToken cancellationToken
    )
    {
        CategoryEntity category = await categoryRepository.GetByIdOrThrowAsync(
            id: query.Id,
            cancellationToken: cancellationToken
        );

        var dto = await categoryDtoFactory.CreateAsync(category, cancellationToken);
        return new AdminGetCategoryByIdResult(Category: dto);
    }
}
