using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Shared.Application.Specifications;

namespace _116.Content.Application.Commerce.Builders;

/// <summary>
/// Interface for building dynamic content order queries using specifications.
/// Implements the Builder pattern to construct complex queries without conditional logic.
/// </summary>
public interface IContentOrderQueryBuilder
{
    /// <summary>
    /// Adds a status filter to the query.
    /// </summary>
    IContentOrderQueryBuilder WithStatus(EnumOrderStatus? status);

    /// <summary>
    /// Adds a customer ID filter to the query.
    /// </summary>
    IContentOrderQueryBuilder WithCustomerId(Guid? customerId);

    /// <summary>
    /// Builds and returns the final specification.
    /// Returns null if no filters were applied.
    /// </summary>
    Specification<ContentOrderEntity>? Build();
}
