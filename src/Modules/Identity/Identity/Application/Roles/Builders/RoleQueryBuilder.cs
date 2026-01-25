using _116.Identity.Application.Roles.Specifications;
using _116.Identity.Domain.Entities;
using _116.Shared.Application.Specifications;

namespace _116.Identity.Application.Roles.Builders;

/// <summary>
/// Builder for constructing dynamic role queries using specifications.
/// Implements the Builder pattern to eliminate conditional logic in query construction.
/// </summary>
public class RoleQueryBuilder : IRoleQueryBuilder
{
    private Specification<RoleEntity>? _specification;

    /// <inheritdoc />
    public IRoleQueryBuilder WithSearch(string? search)
    {
        if (string.IsNullOrWhiteSpace(value: search))
        {
            return this;
        }

        var searchSpec = new RoleSearchSpecification(search: search);
        CombineSpecification(spec: searchSpec);
        return this;
    }

    /// <inheritdoc />
    public IRoleQueryBuilder WithActiveStatus(bool? isActive)
    {
        if (!isActive.HasValue)
        {
            return this;
        }

        Specification<RoleEntity> activeSpec = isActive.Value
            ? new RoleIsActiveSpecification()
            : new RoleIsNotActiveSpecification();

        CombineSpecification(spec: activeSpec);
        return this;
    }

    /// <inheritdoc />
    public IRoleQueryBuilder WithDeletedStatus(bool? isDeleted)
    {
        if (!isDeleted.HasValue)
        {
            return this;
        }

        Specification<RoleEntity> deletedSpec = isDeleted.Value
            ? new RoleIsDeletedSpecification()
            : new RoleNotDeletedSpecification();

        CombineSpecification(spec: deletedSpec);
        return this;
    }

    /// <inheritdoc />
    public Specification<RoleEntity>? Build()
    {
        return _specification;
    }

    private void CombineSpecification(Specification<RoleEntity> spec)
    {
        _specification = _specification is null ? spec : _specification.And(other: spec);
    }
}
