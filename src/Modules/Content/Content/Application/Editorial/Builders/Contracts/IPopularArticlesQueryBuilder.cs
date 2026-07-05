using _116.Content.Domain.Entities;
using _116.Content.Infrastructure.Persistence;

namespace _116.Content.Application.Editorial.Builders.Contracts;

/// <summary>
/// Interface for building the popular-articles query using the builder pattern.
/// Encapsulates the weighted-score ordering and the published/category/exclude filters.
/// </summary>
public interface IPopularArticlesQueryBuilder
{
    /// <summary>
    /// Restricts ranking to a single category.
    /// When not called, all categories are ranked.
    /// </summary>
    IPopularArticlesQueryBuilder WithCategory(Guid? categoryId);

    /// <summary>
    /// Omits a specific article id from the result
    /// (the article being viewed on a detail page).
    /// </summary>
    IPopularArticlesQueryBuilder WithExcludeId(Guid? excludeId);

    /// <summary>
    /// Limits the number of ranked articles returned.
    /// </summary>
    IPopularArticlesQueryBuilder WithLimit(int? limit);

    /// <summary>
    /// Builds and returns the final ordered, filtered, optionally limited <see cref="IQueryable" />.
    /// </summary>
    IQueryable<ArticleEntity> Build(ContentDbContext context);
}
