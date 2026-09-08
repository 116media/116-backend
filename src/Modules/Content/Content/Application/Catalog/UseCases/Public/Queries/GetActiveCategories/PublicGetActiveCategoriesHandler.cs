using _116.Content.Application.Shared.DTOs;
using _116.Content.Application.Shared.Mappers;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Core.Contracts.Application.Services;
using _116.Shared.Contracts.Application.CQRS;
using MapsterMapper;

namespace _116.Content.Application.Catalog.UseCases.Public.Queries.GetActiveCategories;

/// <summary>
/// Handles the <see cref="PublicGetActiveCategoriesQuery" /> to retrieve the list of active public categories.
/// </summary>
/// <param name="categoryRepository">Repository for category data access operations.</param>
/// <param name="fileStorage">Core's storage contract.</param>
/// <param name="mapper">Mapster mapper for entity-to-DTO transformations.</param>
public class PublicGetActiveCategoriesHandler(
    ICategoryRepository categoryRepository,
    IFileStorageService fileStorage,
    IMapper mapper
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

        IReadOnlyList<CategoryDto> dtoList = await categories.ToCategoryDtosAsync(
            mapper,
            fileStorage,
            cancellationToken
        );

        return new PublicGetActiveCategoriesResult(Categories: dtoList);
    }
}
