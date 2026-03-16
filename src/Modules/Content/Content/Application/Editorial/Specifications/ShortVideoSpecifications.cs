using System.Linq.Expressions;
using _116.Content.Domain.Entities;
using _116.Shared.Application.Specifications;
using Microsoft.EntityFrameworkCore;

namespace _116.Content.Application.Editorial.Specifications;

/// <summary>
/// Specification that matches a short video by its unique identifier.
/// </summary>
public class ShortVideoByIdSpecification(Guid id) : Specification<ShortVideoEntity>
{
    /// <inheritdoc />
    public override Expression<Func<ShortVideoEntity, bool>> ToExpression()
    {
        return shortVideo => shortVideo.Id == id;
    }
}

/// <summary>
/// Specification for full-text search across short video Title and Description fields.
/// Uses case-insensitive matching (ILIKE in PostgreSQL).
/// </summary>
public class ShortVideoSearchSpecification(string search) : Specification<ShortVideoEntity>
{
    /// <inheritdoc />
    public override Expression<Func<ShortVideoEntity, bool>> ToExpression()
    {
        string pattern = $"%{search}%";
        return shortVideo => EF.Functions.ILike(shortVideo.Title, pattern);
    }
}

/// <summary>
/// Specification that matches a short video by its URL slug.
/// </summary>
public class ShortVideoBySlugSpecification(string slug) : Specification<ShortVideoEntity>
{
    /// <inheritdoc />
    public override Expression<Func<ShortVideoEntity, bool>> ToExpression()
    {
        return shortVideo => shortVideo.Slug == slug;
    }
}

/// <summary>
/// Specification that matches only active short videos.
/// </summary>
public class ActiveShortVideoSpecification : Specification<ShortVideoEntity>
{
    /// <inheritdoc />
    public override Expression<Func<ShortVideoEntity, bool>> ToExpression()
    {
        return shortVideo => shortVideo.IsActive;
    }
}
