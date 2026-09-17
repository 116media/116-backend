using _116.Content.Application.Shared.DTOs;
using _116.Content.Domain.Entities;

namespace _116.Content.Application.Catalog.Factories;

/// <summary>
/// Builds <see cref="CategoryDto" /> projections, resolving each category's poster in a single
/// batch so callers never issue one file query per category.
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
}
