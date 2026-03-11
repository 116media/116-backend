using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Shared.Application.Specifications;

namespace _116.Content.Application.Editorial.Builders;

/// <summary>
/// Interface for building dynamic video queries using specifications.
/// Implements the Builder pattern to construct complex queries without conditional logic.
/// </summary>
public interface IVideoQueryBuilder
{
    /// <summary>
    /// Adds a full-text search filter across Title, Description, MetaTitle, and MetaDescription.
    /// </summary>
    IVideoQueryBuilder WithSearch(string? search);

    /// <summary>
    /// Adds a content status filter to the query.
    /// </summary>
    IVideoQueryBuilder WithStatus(EnumContentStatus? status);

    /// <summary>
    /// Adds a category filter to the query.
    /// </summary>
    IVideoQueryBuilder WithCategory(Guid? categoryId);

    /// <summary>
    /// Builds and returns the final specification.
    /// Returns null if no filters were applied.
    /// </summary>
    Specification<VideoEntity>? Build();
}
