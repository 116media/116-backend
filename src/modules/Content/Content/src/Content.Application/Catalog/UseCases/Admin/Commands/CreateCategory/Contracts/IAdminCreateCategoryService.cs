using _116.Content.Domain.Entities;

namespace _116.Content.Application.Catalog.UseCases.Admin.Commands.CreateCategory.Contracts;

/// <summary>
/// Resolves and applies a category creation: validates the content type and the slug, hands the
/// exclusive flag over from its current holder, and stages the new category.
/// </summary>
public interface IAdminCreateCategoryService
{
    /// <summary>
    /// Stages the new category, throwing the localized error when the slug is taken or the
    /// exclusive flag is requested for a non-video type. The caller owns the commit.
    /// </summary>
    /// <param name="command">The creation command.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>The staged category.</returns>
    Task<CategoryEntity> CreateAsync(AdminCreateCategoryCommand command, CancellationToken cancellationToken);
}
