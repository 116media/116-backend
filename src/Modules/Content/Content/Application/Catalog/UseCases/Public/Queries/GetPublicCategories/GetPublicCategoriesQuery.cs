using _116.Content.Application.Shared.DTOs;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Catalog.UseCases.Public.Queries.GetPublicCategories;

/// <summary>
/// Query for retrieving the list of active categories visible to the public.
/// </summary>
/// <param name="ContentTypeId">Optional filter to return only categories for a specific content type.</param>
public record GetPublicCategoriesQuery(Guid? ContentTypeId) : IQuery<GetPublicCategoriesResult>;

/// <summary>
/// Result of the <see cref="GetPublicCategoriesQuery" /> containing the list of active categories.
/// </summary>
/// <param name="Categories">The list of active category DTOs.</param>
public record GetPublicCategoriesResult(IReadOnlyList<CategoryDto> Categories);
