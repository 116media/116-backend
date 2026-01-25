using _116.Identity.Application.Roles.Specifications;
using _116.Identity.Domain.Entities;
using _116.Shared.Application.Specifications;

namespace _116.Identity.Application.Roles.Builders;

/// <summary>
/// Builder for constructing dynamic permission queries using specifications.
/// Implements the Builder pattern to eliminate conditional logic in query construction.
/// </summary>
public class PermissionQueryBuilder : IPermissionQueryBuilder
{
    private Specification<PermissionEntity>? _specification;

    /// <inheritdoc />
    public IPermissionQueryBuilder WithSearch(string? search)
    {
        if (string.IsNullOrWhiteSpace(value: search))
        {
            return this;
        }

        var searchSpec = new PermissionSearchSpecification(search: search);
        CombineSpecification(spec: searchSpec);
        return this;
    }

    /// <inheritdoc />
    public IPermissionQueryBuilder WithActiveStatus(bool? isActive)
    {
        if (!isActive.HasValue)
        {
            return this;
        }

        Specification<PermissionEntity> activeSpec = isActive.Value
            ? new PermissionIsActiveSpecification()
            : new PermissionIsNotActiveSpecification();

        CombineSpecification(spec: activeSpec);
        return this;
    }

    /// <inheritdoc />
    public IPermissionQueryBuilder WithDeletedStatus(bool? isDeleted)
    {
        if (!isDeleted.HasValue)
        {
            return this;
        }

        Specification<PermissionEntity> deletedSpec = isDeleted.Value
            ? new PermissionIsDeletedSpecification()
            : new PermissionNotDeletedSpecification();

        CombineSpecification(spec: deletedSpec);
        return this;
    }

    /// <inheritdoc />
    public Specification<PermissionEntity>? Build()
    {
        return _specification;
    }

    private void CombineSpecification(Specification<PermissionEntity> spec)
    {
        _specification = _specification is null ? spec : _specification.And(other: spec);
    }
}
