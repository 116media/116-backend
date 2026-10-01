using _116.Content.Domain.Entities;

namespace _116.Content.Application.Catalog.UseCases.Admin.Commands.UpdateCategory.Contracts;

/// <summary>
/// Resolves a category for an update: loads it and gates the slug, the exclusive flag and the
/// default-for-lyrics flag against its state and content type.
/// </summary>
public interface IAdminUpdateCategoryService
{
    /// <summary>
    /// Loads the category, throwing the localized error when the slug is taken or a requested
    /// flag is not allowed for its state or type. The caller applies the update and owns the commit.
    /// </summary>
    /// <param name="command">The update command.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>The category to update.</returns>
    Task<CategoryEntity> EnsureUpdatableAsync(AdminUpdateCategoryCommand command, CancellationToken cancellationToken);
}
