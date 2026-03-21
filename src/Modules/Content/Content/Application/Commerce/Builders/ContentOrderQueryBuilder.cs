using _116.Content.Application.Commerce.Specifications;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Shared.Application.Specifications;

namespace _116.Content.Application.Commerce.Builders;

/// <summary>
/// Builder for constructing dynamic content order queries using specifications.
/// Implements the Builder pattern to eliminate conditional logic in query construction.
/// </summary>
public class ContentOrderQueryBuilder : IContentOrderQueryBuilder
{
    private Specification<ContentOrderEntity>? _specification;

    /// <inheritdoc />
    public IContentOrderQueryBuilder WithStatus(EnumOrderStatus? status)
    {
        if (!status.HasValue)
        {
            return this;
        }

        var statusSpec = new ContentOrderByStatusSpecification(status: status.Value);
        CombineSpecification(spec: statusSpec);
        return this;
    }

    /// <inheritdoc />
    public IContentOrderQueryBuilder WithCustomerId(Guid? customerId)
    {
        if (!customerId.HasValue)
        {
            return this;
        }

        var customerSpec = new ContentOrderByCustomerIdSpecification(customerId: customerId.Value);
        CombineSpecification(spec: customerSpec);
        return this;
    }

    /// <inheritdoc />
    public Specification<ContentOrderEntity>? Build()
    {
        return _specification;
    }

    private void CombineSpecification(Specification<ContentOrderEntity> spec)
    {
        _specification = _specification is null ? spec : _specification.And(other: spec);
    }
}
