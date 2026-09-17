using _116.Content.Application.Shared.DTOs;
using _116.Content.Application.Shared.Mappers;
using _116.Content.Domain.Entities;

namespace _116.Content.Application.Catalog.Factories;

/// <summary>
/// Builds <see cref="CategoryDto" /> projections, resolving each category's poster, content type
/// and pricing tiers in a single batch so callers never issue one query per category.
/// </summary>
public interface ICategoryDtoFactory
{
    /// <summary>
    /// Builds the projection for one category.
    /// </summary>
    /// <param name="category">The category to project.</param>
    /// <param name="ct">Token to observe for cancellation requests.</param>
    /// <returns>The projection.</returns>
    Task<CategoryDto> CreateAsync(CategoryEntity category, CancellationToken ct = default);

    /// <summary>
    /// Builds the projections for a list of categories, resolving every poster in one query.
    /// </summary>
    /// <param name="categories">The categories to project.</param>
    /// <param name="ct">Token to observe for cancellation requests.</param>
    /// <returns>The projections, in the order supplied.</returns>
    Task<IReadOnlyList<CategoryDto>> CreateManyAsync(
        IReadOnlyList<CategoryEntity> categories,
        CancellationToken ct = default
    );

    /// <summary>
    /// Resolves every row a set of categories names — posters, content types and pricing
    /// tiers — in one batch, for a caller assembling several projections at once.
    /// </summary>
    /// <param name="categories">The categories to resolve lookups for.</param>
    /// <param name="ct">Token to observe for cancellation requests.</param>
    /// <returns>The resolved lookups.</returns>
    Task<CategoryLookups> ResolveLookupsAsync(IReadOnlyList<CategoryEntity> categories, CancellationToken ct = default);
}
