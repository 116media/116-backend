using System.Linq.Expressions;
using _116.Content.Domain.Entities;
using _116.Shared.Application.Specifications;
using Microsoft.EntityFrameworkCore;

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

/// <summary>
/// Specification for fuzzy search across tag Name and Slug.
/// Uses case-insensitive matching (ILIKE in PostgreSQL).
/// </summary>
public class TagSearchSpecification(string search) : Specification<TagEntity>
{
    /// <inheritdoc />
    public override Expression<Func<TagEntity, bool>> ToExpression()
    {
        string pattern = $"%{search}%";
        return tag => EF.Functions.ILike(tag.Name, pattern) || EF.Functions.ILike(tag.Slug, pattern);
    }
}
