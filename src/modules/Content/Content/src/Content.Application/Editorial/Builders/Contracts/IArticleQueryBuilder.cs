using _116.BuildingBlocks.Domain.Specifications;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;

namespace _116.Content.Application.Editorial.Builders.Contracts;

/// <summary>
/// Interface for building dynamic article queries using specifications.
/// Implements the Builder pattern to construct complex queries without conditional logic.
/// </summary>
public interface IArticleQueryBuilder
{
    /// <summary>
    /// Adds a full-text search filter across Title, Headline, Body, MetaTitle, and MetaDescription.
    /// </summary>
    IArticleQueryBuilder WithSearch(string? search);

    /// <summary>
    /// Adds a content status filter to the query.
    /// </summary>
    IArticleQueryBuilder WithStatus(EnumContentStatus? status);

    /// <summary>
    /// Adds a category filter to the query.
    /// </summary>
    IArticleQueryBuilder WithCategory(Guid? categoryId);

    /// <summary>
    /// Adds a category-slug filter to the query.
    /// </summary>
    IArticleQueryBuilder WithCategorySlug(string? categorySlug);

    /// <summary>
    /// Adds a tag-slug filter to the query.
    /// </summary>
    IArticleQueryBuilder WithTag(string? tagSlug);

    /// <summary>
    /// Builds and returns the final specification.
    /// Returns null if no filters were applied.
    /// </summary>
    /// <param name="tags">The tag rows the tag-slug filter probes.</param>
    /// <param name="categories">The category rows the category-slug filter probes.</param>
    Specification<ArticleEntity>? Build(IQueryable<TagEntity> tags, IQueryable<CategoryEntity> categories);
}
