using _116.Content.Application.Commerce.Specifications;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Shared.Application.Specifications;

namespace _116.Content.Application.Commerce.Builders;

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

/// <summary>
/// Builder for constructing dynamic content payment queries using specifications.
/// Implements the Builder pattern to eliminate conditional logic in query construction.
/// </summary>
public class ContentPaymentQueryBuilder : IContentPaymentQueryBuilder
{
    private Specification<ContentPaymentEntity>? _specification;

    /// <inheritdoc />
    public IContentPaymentQueryBuilder WithStatus(EnumPaymentStatus? status)
    {
        if (!status.HasValue)
        {
            return this;
        }

        var statusSpec = new ContentPaymentByStatusSpecification(status: status.Value);
        CombineSpecification(spec: statusSpec);
        return this;
    }

    /// <inheritdoc />
    public IContentPaymentQueryBuilder WithMethod(EnumPaymentMethod? method)
    {
        if (!method.HasValue)
        {
            return this;
        }

        var methodSpec = new ContentPaymentByMethodSpecification(method: method.Value);
        CombineSpecification(spec: methodSpec);
        return this;
    }

    /// <inheritdoc />
    public IContentPaymentQueryBuilder WithSearch(string? search)
    {
        if (string.IsNullOrWhiteSpace(value: search))
        {
            return this;
        }

        var searchSpec = new ContentPaymentSearchSpecification(search: search);
        CombineSpecification(spec: searchSpec);
        return this;
    }

    /// <inheritdoc />
    public Specification<ContentPaymentEntity>? Build()
    {
        return _specification;
    }

    private void CombineSpecification(Specification<ContentPaymentEntity> spec)
    {
        _specification = _specification is null ? spec : _specification.And(other: spec);
    }
}
