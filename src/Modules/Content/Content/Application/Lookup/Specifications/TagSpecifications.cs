using System.Linq.Expressions;
using _116.Content.Domain.Entities;
using _116.Shared.Application.Specifications;

namespace _116.Content.Application.Lookup.Specifications;

/// <summary>
/// Specification that matches a tag by its URL-safe slug.
/// </summary>
public class TagBySlugSpecification(string slug) : Specification<TagEntity>
{
    /// <inheritdoc />
    public override Expression<Func<TagEntity, bool>> ToExpression()
    {
        return tag => tag.Slug == slug;
    }
}

/// <summary>
/// Specification that matches a tag by its name.
/// </summary>
public class TagByNameSpecification(string name) : Specification<TagEntity>
{
    /// <inheritdoc />
    public override Expression<Func<TagEntity, bool>> ToExpression()
    {
        return tag => tag.Name == name;
    }
}
