using _116.Content.Application.Editorial.Specifications;
using _116.Content.Domain.Entities;
using _116.Shared.Application.Specifications;

namespace _116.Content.Application.Editorial.Builders;

/// <summary>
/// Builder for constructing dynamic short video queries using specifications.
/// Implements the Builder pattern to eliminate conditional logic in query construction.
/// </summary>
public class ShortVideoQueryBuilder : IShortVideoQueryBuilder
{
    private Specification<ShortVideoEntity>? _specification;

    /// <inheritdoc />
    public IShortVideoQueryBuilder WithSearch(string? search)
    {
        if (string.IsNullOrWhiteSpace(value: search))
        {
            return this;
        }

        var searchSpec = new ShortVideoSearchSpecification(search: search);
        CombineSpecification(spec: searchSpec);
        return this;
    }

    /// <inheritdoc />
    public IShortVideoQueryBuilder WithIsActive(bool? isActive)
    {
        if (!isActive.HasValue)
        {
            return this;
        }

        Specification<ShortVideoEntity> activeSpec = isActive.Value
            ? new ActiveShortVideoSpecification()
            : new ActiveShortVideoSpecification().Not();
        CombineSpecification(spec: activeSpec);
        return this;
    }

    /// <inheritdoc />
    public Specification<ShortVideoEntity>? Build()
    {
        return _specification;
    }

    private void CombineSpecification(Specification<ShortVideoEntity> spec)
    {
        _specification = _specification is null ? spec : _specification.And(other: spec);
    }
}
