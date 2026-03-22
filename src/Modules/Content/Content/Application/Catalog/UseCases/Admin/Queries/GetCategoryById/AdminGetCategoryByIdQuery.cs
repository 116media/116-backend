using _116.Content.Application.Shared.DTOs;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Catalog.UseCases.Admin.Queries.GetCategoryById;

/// <summary>
/// Query for retrieving a single category by its identifier, including its pricing configuration.
/// </summary>
/// <param name="Id">The unique identifier of the category.</param>
public record AdminGetCategoryByIdQuery(Guid Id) : IQuery<AdminGetCategoryByIdResult>;

/// <summary>
/// Result of the <see cref="AdminGetCategoryByIdQuery" /> containing the category details.
/// </summary>
/// <param name="Category">The category information including pricing tiers.</param>
public record AdminGetCategoryByIdResult(CategoryDto Category);
