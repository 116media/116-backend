using System.Linq.Expressions;
using _116.Content.Domain.Entities;
using _116.Shared.Application.Specifications;

namespace _116.Content.Application.Lookup.Specifications;

/// <summary>
/// Specification that matches a promotion level by its unique identifier.
/// </summary>
public class PromotionLevelByIdSpecification(Guid id) : Specification<PromotionLevelEntity>
{
    /// <inheritdoc />
    public override Expression<Func<PromotionLevelEntity, bool>> ToExpression()
    {
        return promotionLevel => promotionLevel.Id == id;
    }
}

/// <summary>
/// Specification that matches a promotion level by its name.
/// </summary>
public class PromotionLevelByNameSpecification(string name) : Specification<PromotionLevelEntity>
{
    /// <inheritdoc />
    public override Expression<Func<PromotionLevelEntity, bool>> ToExpression()
    {
        return promotionLevel => promotionLevel.Name == name;
    }
}

/// <summary>
/// Specification that matches only active promotion levels.
/// Used for public-facing queries where inactive levels must be hidden.
/// </summary>
public class ActivePromotionLevelSpecification : Specification<PromotionLevelEntity>
{
    /// <inheritdoc />
    public override Expression<Func<PromotionLevelEntity, bool>> ToExpression()
    {
        return promotionLevel => promotionLevel.IsActive;
    }
}
