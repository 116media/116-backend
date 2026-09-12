using _116.Content.Domain.Entities;

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
    /// Builds the final ordered, filtered, optionally limited query over the supplied source.
    /// The caller owns the source and any eager loading on it, so the ranking policy here stays
    /// free of persistence concerns.
    /// </summary>
    /// <param name="source">The articles to rank.</param>
    /// <returns>The ranked query.</returns>
    IQueryable<ArticleEntity> Build(IQueryable<ArticleEntity> source);
}
