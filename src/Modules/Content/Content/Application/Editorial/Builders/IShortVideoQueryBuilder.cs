using _116.Content.Domain.Entities;
using _116.Shared.Application.Specifications;

namespace _116.Content.Application.Editorial.Builders;

/// <summary>
/// Interface for building dynamic short video queries using specifications.
/// Implements the Builder pattern to construct complex queries without conditional logic.
/// </summary>
public interface IShortVideoQueryBuilder
{
    /// <summary>
    /// Adds a full-text search filter across Title and Description.
    /// </summary>
    IShortVideoQueryBuilder WithSearch(string? search);

    /// <summary>
    /// Adds an active status filter to the query.
    /// </summary>
    IShortVideoQueryBuilder WithIsActive(bool? isActive);

    /// <summary>
    /// Builds and returns the final specification.
    /// Returns null if no filters were applied.
    /// </summary>
    Specification<ShortVideoEntity>? Build();
}
