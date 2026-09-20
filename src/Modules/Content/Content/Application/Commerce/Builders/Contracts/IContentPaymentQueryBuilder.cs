using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Shared.Application.Specifications;

namespace _116.Content.Application.Commerce.Builders.Contracts;

/// <summary>
/// Interface for building the admin payments listing over its root: orders carrying a payment.
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
    /// Adds a filter matching the ordering customer's name, email, or company.
    /// </summary>
    IContentPaymentQueryBuilder WithSearch(string? search);

    /// <summary>
    /// Builds and returns the final specification over orders carrying a payment, resolving the
    /// customer search against the supplied customer rows. Returns null if no filters were applied.
    /// </summary>
    /// <param name="customers">The customer rows the customer search probes.</param>
    Specification<ContentOrderEntity>? Build(IQueryable<CustomerEntity> customers);
}
