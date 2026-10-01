using _116.Content.Domain.Entities;

namespace _116.Content.Application.Catalog.UseCases.Admin.Commands.PinCategoryToFeed.Contracts;

/// <summary>
/// Resolves and applies a feed pin: gates the category's state, type and published volume, evicts
/// the oldest pin when the cap is reached, and pins through the category aggregate.
/// </summary>
public interface IAdminPinCategoryToFeedService
{
    /// <summary>
    /// Pins the category, throwing the localized error when it is inactive, not a video category
    /// or too thin to fill a section. The caller owns the commit.
    /// </summary>
    /// <param name="categoryId">The category being pinned.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>The pinned category.</returns>
    Task<CategoryEntity> PinAsync(Guid categoryId, CancellationToken cancellationToken);
}
