using System.Linq.Expressions;
using _116.Content.Domain.Entities;
using _116.Shared.Application.Specifications;
using Microsoft.EntityFrameworkCore;

namespace _116.Content.Application.Catalog.Specifications;

/// <summary>
/// Specification that matches a category by its unique identifier.
/// </summary>
public class CategoryByIdSpecification(Guid id) : Specification<CategoryEntity>
{
    /// <inheritdoc />
    public override Expression<Func<CategoryEntity, bool>> ToExpression()
    {
        return category => category.Id == id;
    }
}

/// <summary>
/// Specification that matches a category by its URL-safe slug (case-insensitive).
/// </summary>
public class CategoryBySlugSpecification(string slug) : Specification<CategoryEntity>
{
    /// <inheritdoc />
    public override Expression<Func<CategoryEntity, bool>> ToExpression()
    {
        return category => EF.Functions.ILike(category.Slug, slug);
    }
}

/// <summary>
/// Specification that matches only active categories.
/// Used for public-facing queries and order creation where inactive categories must be hidden.
/// </summary>
public class ActiveCategorySpecification : Specification<CategoryEntity>
{
    /// <inheritdoc />
    public override Expression<Func<CategoryEntity, bool>> ToExpression()
    {
        return category => category.IsActive;
    }
}

/// <summary>
/// Specification that matches only free categories (no payment required).
/// </summary>
public class FreeCategorySpecification : Specification<CategoryEntity>
{
    /// <inheritdoc />
    public override Expression<Func<CategoryEntity, bool>> ToExpression()
    {
        return category => category.IsFree;
    }
}

/// <summary>
/// Specification that matches only paid categories.
/// </summary>
public class PaidCategorySpecification : Specification<CategoryEntity>
{
    /// <inheritdoc />
    public override Expression<Func<CategoryEntity, bool>> ToExpression()
    {
        return category => !category.IsFree;
    }
}

/// <summary>
/// Specification that matches categories belonging to a specific content type.
/// </summary>
public class CategoryByContentTypeSpecification(Guid contentTypeId) : Specification<CategoryEntity>
{
    /// <inheritdoc />
    public override Expression<Func<CategoryEntity, bool>> ToExpression()
    {
        return category => category.ContentTypeId == contentTypeId;
    }
}
