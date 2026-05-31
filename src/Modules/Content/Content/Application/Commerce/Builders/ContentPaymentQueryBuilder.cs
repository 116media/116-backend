using _116.Content.Application.Commerce.Builders.Contracts;
using _116.Content.Application.Commerce.Specifications;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Shared.Application.Specifications;

namespace _116.Content.Application.Commerce.Builders;

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
