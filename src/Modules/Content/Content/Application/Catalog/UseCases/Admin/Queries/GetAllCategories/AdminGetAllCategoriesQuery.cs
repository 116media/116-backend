using _116.Content.Application.Shared.DTOs;
using _116.Shared.Application.Pagination;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Catalog.UseCases.Admin.Queries.GetAllCategories;

/// <summary>
/// Query for retrieving a paginated list of categories with optional filters.
/// </summary>
/// <param name="PaginatedRequest">Pagination parameters.</param>
/// <param name="IsActive">Optional filter by active status.</param>
/// <param name="IsFree">Optional filter by free/paid status.</param>
public record AdminGetAllCategoriesQuery(PaginatedRequest PaginatedRequest, bool? IsActive = null, bool? IsFree = null)
    : IQuery<AdminGetAllCategoriesResult>;

/// <summary>
/// Result of the <see cref="AdminGetAllCategoriesQuery" /> containing paginated category DTOs.
/// </summary>
/// <param name="Categories">Paginated result containing category DTOs.</param>
public record AdminGetAllCategoriesResult(PaginatedResult<CategoryDto> Categories);
