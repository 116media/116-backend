using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Shared.Application.Specifications;

namespace _116.Content.Application.Commerce.Builders.Contracts;

/// <summary>
/// Interface for building dynamic content payment queries using specifications.
/// Implements the Builder pattern to construct complex queries without conditional logic.
/// </summary>
public interface IContentPaymentQueryBuilder
{
    /// <summary>
    /// Adds a payment status filter to the query.
    /// </summary>
    IContentPaymentQueryBuilder WithStatus(EnumPaymentStatus? status);

    /// <summary>
    /// Adds a payment method filter to the query.
    /// </summary>
    IContentPaymentQueryBuilder WithMethod(EnumPaymentMethod? method);

    /// <summary>
    /// Adds a search filter matching customer name, email, or company.
    /// </summary>
    IContentPaymentQueryBuilder WithSearch(string? search);

    /// <summary>
    /// Builds and returns the final specification.
    /// Returns null if no filters were applied.
    /// </summary>
    Specification<ContentPaymentEntity>? Build();
}
