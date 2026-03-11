using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Shared.Application.Specifications;

namespace _116.Content.Application.Editorial.Builders;

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
    /// Builds and returns the final specification.
    /// Returns null if no filters were applied.
    /// </summary>
    Specification<ArticleEntity>? Build();
}
