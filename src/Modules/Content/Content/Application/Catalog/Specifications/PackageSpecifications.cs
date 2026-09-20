using System.Linq.Expressions;
using _116.Content.Domain.Entities;
using _116.Shared.Application.Specifications;

namespace _116.Content.Application.Catalog.Specifications;

/// <summary>
/// Specification that matches only active packages.
/// Used when listing packages available for new orders.
/// </summary>
public class ActivePackageSpecification : Specification<PackageEntity>
{
    /// <inheritdoc />
    public override Expression<Func<PackageEntity, bool>> ToExpression()
    {
        return package => package.IsActive;
    }
}

/// <summary>
/// Specification that matches only inactive packages.
/// </summary>
public class InactivePackageSpecification : Specification<PackageEntity>
{
    /// <inheritdoc />
    public override Expression<Func<PackageEntity, bool>> ToExpression()
    {
        return package => !package.IsActive;
    }
}
