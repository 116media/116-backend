using _116.Content.Application.Shared.Mappers;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Shared.Contracts.Application.CQRS;
using MapsterMapper;

namespace _116.Content.Application.Catalog.UseCases.Admin.Queries.GetCategoryById;

/// <summary>
/// Handles the <see cref="GetCategoryByIdQuery" /> to retrieve a category by its identifier.
/// </summary>
/// <param name="categoryRepository">Repository for category data access operations.</param>
/// <param name="mapper">Mapster mapper for entity-to-DTO transformations.</param>
public class GetCategoryByIdHandler(ICategoryRepository categoryRepository, IMapper mapper)
    : IQueryHandler<GetCategoryByIdQuery, GetCategoryByIdResult>
{
    /// <inheritdoc />
    public async Task<GetCategoryByIdResult> Handle(GetCategoryByIdQuery query, CancellationToken cancellationToken)
    {
        Guid id = Guid.Parse(query.Id);

        CategoryEntity category = await categoryRepository.GetByIdOrThrowAsync(
            id: id,
            cancellationToken: cancellationToken
        );

        var dto = category.ToCategoryDto(mapper);
        return new GetCategoryByIdResult(Category: dto);
    }
}
