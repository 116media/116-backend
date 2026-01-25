using _116.Identity.Domain.Entities;
using _116.Shared.Application.Specifications;

namespace _116.Identity.Application.Roles.Builders;

/// <summary>
/// Interface for building dynamic role queries using specifications.
/// Implements the Builder pattern to construct complex queries without conditional logic.
/// </summary>
public interface IRoleQueryBuilder
{
    /// <summary>
    /// Adds search filter to the query (fuzzy match on Name and Description).
    /// </summary>
    IRoleQueryBuilder WithSearch(string? search);

    /// <summary>
    /// Adds active status filter to the query.
    /// </summary>
    IRoleQueryBuilder WithActiveStatus(bool? isActive);

    /// <summary>
    /// Adds deleted status filter to the query.
    /// </summary>
    IRoleQueryBuilder WithDeletedStatus(bool? isDeleted);

    /// <summary>
    /// Builds and returns the final specification.
    /// Returns null if no filters were applied.
    /// </summary>
    Specification<RoleEntity>? Build();
}
