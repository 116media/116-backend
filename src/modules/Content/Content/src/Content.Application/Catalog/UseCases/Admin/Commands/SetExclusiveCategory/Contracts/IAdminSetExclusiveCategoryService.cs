using _116.Content.Domain.Entities;

namespace _116.Content.Application.Catalog.UseCases.Admin.Commands.SetExclusiveCategory.Contracts;

/// <summary>
/// Resolves a category for the exclusive flag: loads it and gates its state and content type.
/// </summary>
public interface IAdminSetExclusiveCategoryService
{
    /// <summary>
    /// Loads the category, throwing the localized error when it is inactive or not a video
    /// category. The caller applies the flag and owns the commit.
    /// </summary>
    /// <param name="categoryId">The category to make exclusive.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>The eligible category.</returns>
    Task<CategoryEntity> EnsureExclusivableAsync(Guid categoryId, CancellationToken cancellationToken);
}
