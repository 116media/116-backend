using _116.Content.Application.Shared.DTOs;
using _116.Content.Application.Shared.Mappers;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Shared.Application.Pagination;
using _116.Shared.Contracts.Application.CQRS;
using MapsterMapper;

namespace _116.Content.Application.Catalog.UseCases.Admin.Queries.GetAllCategories;

/// <summary>
/// Handles the <see cref="AdminGetAllCategoriesQuery" /> to retrieve a paginated list of categories.
/// </summary>
/// <param name="categoryRepository">Repository for category data access operations.</param>
/// <param name="mapper">Mapster mapper for entity-to-DTO transformations.</param>
public class AdminGetAllCategoriesHandler(ICategoryRepository categoryRepository, IMapper mapper)
    : IQueryHandler<AdminGetAllCategoriesQuery, AdminGetAllCategoriesResult>
{
    /// <inheritdoc />
    public async Task<AdminGetAllCategoriesResult> Handle(
        AdminGetAllCategoriesQuery query,
        CancellationToken cancellationToken
    )
    {
        int pageSize = query.PaginatedRequest.PageSize;
        int pageIndex = query.PaginatedRequest.PageIndex;

        (List<CategoryEntity> categories, int totalCount) = await categoryRepository.GetAllAsync(
            page: pageIndex + 1,
            pageSize: pageSize,
            isActive: query.IsActive,
            isFree: query.IsFree,
            cancellationToken: cancellationToken
        );

        List<CategoryDto> dtoList = categories.Select(c => c.ToCategoryDto(mapper)).ToList();

        var paginatedResult = new PaginatedResult<CategoryDto>(
            pageIndex: pageIndex,
            pageSize: pageSize,
            count: totalCount,
            items: dtoList
        );

        return new AdminGetAllCategoriesResult(Categories: paginatedResult);
    }
}
