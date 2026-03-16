using System.Linq.Expressions;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Shared.Application.Specifications;
using Microsoft.EntityFrameworkCore;

namespace _116.Content.Application.Editorial.Specifications;

/// <summary>
/// Specification that matches a video by its unique identifier.
/// </summary>
public class VideoByIdSpecification(Guid id) : Specification<VideoEntity>
{
    /// <inheritdoc />
    public override Expression<Func<VideoEntity, bool>> ToExpression()
    {
        return video => video.Id == id;
    }
}

/// <summary>
/// Specification that matches a video by its URL-safe slug (case-insensitive).
/// </summary>
public class VideoBySlugSpecification(string slug) : Specification<VideoEntity>
{
    /// <inheritdoc />
    public override Expression<Func<VideoEntity, bool>> ToExpression()
    {
        return video => EF.Functions.ILike(video.Slug, slug);
    }
}

/// <summary>
/// Specification that matches videos by their content status.
/// </summary>
public class VideoByStatusSpecification(EnumContentStatus status) : Specification<VideoEntity>
{
    /// <inheritdoc />
    public override Expression<Func<VideoEntity, bool>> ToExpression()
    {
        return video => video.Status == status;
    }
}

/// <summary>
/// Specification that matches videos belonging to a specific category.
/// </summary>
public class VideoByCategorySpecification(Guid categoryId) : Specification<VideoEntity>
{
    /// <inheritdoc />
    public override Expression<Func<VideoEntity, bool>> ToExpression()
    {
        return video => video.CategoryId == categoryId;
    }
}

/// <summary>
/// Specification for full-text search across video Title, Description,
/// MetaTitle, and MetaDescription fields.
/// Uses case-insensitive matching (ILIKE in PostgreSQL).
/// </summary>
public class VideoSearchSpecification(string search) : Specification<VideoEntity>
{
    /// <inheritdoc />
    public override Expression<Func<VideoEntity, bool>> ToExpression()
    {
        string pattern = $"%{search}%";
        return video =>
            EF.Functions.ILike(video.Title, pattern)
            || (video.Description != null && EF.Functions.ILike(video.Description, pattern))
            || (video.MetaTitle != null && EF.Functions.ILike(video.MetaTitle, pattern))
            || (video.MetaDescription != null && EF.Functions.ILike(video.MetaDescription, pattern));
    }
}

/// <summary>
/// Specification that matches videos that are currently featured and published.
/// Featured videos must have <c>IsFeatured = true</c>, a future or null <c>FeaturedUntil</c>,
/// and <c>Status = Published</c>.
/// </summary>
public class FeaturedVideoSpecification : Specification<VideoEntity>
{
    /// <inheritdoc />
    public override Expression<Func<VideoEntity, bool>> ToExpression()
    {
        DateTimeOffset now = DateTimeOffset.UtcNow;
        return video =>
            video.IsFeatured
            && video.Status == EnumContentStatus.Published
            && (video.FeaturedUntil == null || video.FeaturedUntil > now);
    }
}
