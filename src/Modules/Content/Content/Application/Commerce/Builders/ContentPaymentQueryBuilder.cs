using _116.Content.Application.Commerce.Builders.Contracts;
using _116.Content.Application.Commerce.Specifications;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Shared.Application.Specifications;

namespace _116.Content.Application.Commerce.Builders;

/// <summary>
/// Builder for the admin payments listing over its root: orders carrying a payment.
/// Implements the Builder pattern to eliminate conditional logic in query construction.
/// </summary>
public class ContentPaymentQueryBuilder : IContentPaymentQueryBuilder
{
    private Specification<ContentOrderEntity>? _specification;
    private string? _search;

    /// <inheritdoc />
    public IContentPaymentQueryBuilder WithStatus(EnumPaymentStatus? status)
    {
        if (!status.HasValue)
        {
            return this;
        }

        var statusSpec = new OrderPaymentByStatusSpecification(status: status.Value);
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

        var methodSpec = new OrderPaymentByMethodSpecification(method: method.Value);
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

        _search = search;
        return this;
    }

    /// <inheritdoc />
    public Specification<ContentOrderEntity>? Build(IQueryable<CustomerEntity> customers)
    {
        if (_search is null)
        {
            return _specification;
        }

        var searchSpec = new ContentOrderSearchSpecification(search: _search, customers: customers);

        return _specification is null ? searchSpec : _specification.And(other: searchSpec);
    }

    private void CombineSpecification(Specification<ContentOrderEntity> spec)
    {
        _specification = _specification is null ? spec : _specification.And(other: spec);
    }
}
