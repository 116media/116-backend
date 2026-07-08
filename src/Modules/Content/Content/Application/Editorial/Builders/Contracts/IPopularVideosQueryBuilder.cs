using _116.Content.Domain.Entities;
using _116.Content.Infrastructure.Persistence;

namespace _116.Content.Application.Editorial.Builders.Contracts;

/// <summary>
/// Interface for building the popular-videos query using the builder pattern.
/// Encapsulates the weighted-score ordering and the published/category/exclude filters.
/// </summary>
public interface IPopularVideosQueryBuilder
{
    /// <summary>
    /// Restricts ranking to a single category.
    /// When not called, all categories are ranked.
    /// </summary>
    IPopularVideosQueryBuilder WithCategory(Guid? categoryId);

    /// <summary>
    /// Omits a specific video id from the result
    /// (the video being viewed on a detail page).
    /// </summary>
    IPopularVideosQueryBuilder WithExcludeId(Guid? excludeId);

    /// <summary>
    /// Limits the number of ranked videos returned.
    /// </summary>
    IPopularVideosQueryBuilder WithLimit(int? limit);

    /// <summary>
    /// Builds and returns the final ordered, filtered, optionally limited <see cref="IQueryable" />.
    /// </summary>
    IQueryable<VideoEntity> Build(ContentDbContext context);
}
