using System.Linq.Expressions;
using _116.Content.Domain.Entities;
using _116.Shared.Application.Specifications;

namespace _116.Content.Application.Catalog.Specifications;

/// <summary>
/// Specification that matches a package by its unique identifier.
/// </summary>
public class PackageByIdSpecification(Guid id) : Specification<PackageEntity>
{
    /// <inheritdoc />
    public override Expression<Func<PackageEntity, bool>> ToExpression()
    {
        return package => package.Id == id;
    }
}

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

/// <summary>
/// Specification that matches a package slot by its unique identifier.
/// </summary>
public class PackageSlotByIdSpecification(Guid slotId) : Specification<PackageSlotEntity>
{
    /// <inheritdoc />
    public override Expression<Func<PackageSlotEntity, bool>> ToExpression()
    {
        return slot => slot.Id == slotId;
    }
}
