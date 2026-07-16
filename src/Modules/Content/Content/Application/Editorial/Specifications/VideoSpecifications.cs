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
/// Specification that matches videos carrying a tag with the given slug (case-insensitive).
/// </summary>
public class VideoByTagSlugSpecification(string tagSlug) : Specification<VideoEntity>
{
    /// <inheritdoc />
    public override Expression<Func<VideoEntity, bool>> ToExpression()
    {
        return video => video.Tags.Any(videoTag => EF.Functions.ILike(videoTag.Tag.Slug, tagSlug));
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
/// Specification that matches videos that currently have an active paid promotion and are published.
/// Promoted videos must have <c>IsPromoted = true</c>, a future or null <c>PromotedUntil</c>,
/// and <c>Status = Published</c>.
/// </summary>
public class PromotedVideoSpecification : Specification<VideoEntity>
{
    /// <inheritdoc />
    public override Expression<Func<VideoEntity, bool>> ToExpression()
    {
        DateTimeOffset now = DateTimeOffset.UtcNow;
        return video =>
            video.IsPromoted
            && video.Status == EnumContentStatus.Published
            && (video.PromotedUntil == null || video.PromotedUntil > now);
    }
}

/// <summary>
/// Specification that matches active videos — excludes Archived and Rejected statuses.
/// </summary>
public class ActiveVideoSpecification : Specification<VideoEntity>
{
    /// <inheritdoc />
    public override Expression<Func<VideoEntity, bool>> ToExpression()
    {
        return video => video.Status != EnumContentStatus.Archived && video.Status != EnumContentStatus.Rejected;
    }
}

/// <summary>
/// Specification that matches a video by its associated order item identifier.
/// </summary>
public class VideoByOrderItemIdSpecification(Guid orderItemId) : Specification<VideoEntity>
{
    /// <inheritdoc />
    public override Expression<Func<VideoEntity, bool>> ToExpression()
    {
        return video => video.OrderItemId == orderItemId;
    }
}

/// <summary>
/// Specification that matches video tags belonging to a specific video.
/// </summary>
public class VideoTagByVideoIdSpecification(Guid videoId) : Specification<VideoTagEntity>
{
    /// <inheritdoc />
    public override Expression<Func<VideoTagEntity, bool>> ToExpression()
    {
        return tag => tag.VideoId == videoId;
    }
}

/// <summary>
/// Specification that matches a video rating by user and video identifiers.
/// </summary>
public class VideoRatingByUserAndVideoSpecification(Guid userId, Guid videoId) : Specification<VideoRatingEntity>
{
    /// <inheritdoc />
    public override Expression<Func<VideoRatingEntity, bool>> ToExpression()
    {
        return rating => rating.UserId == userId && rating.VideoId == videoId;
    }
}

/// <summary>
/// Specification that matches all video ratings belonging to a specific video.
/// </summary>
public class VideoRatingByVideoIdSpecification(Guid videoId) : Specification<VideoRatingEntity>
{
    /// <inheritdoc />
    public override Expression<Func<VideoRatingEntity, bool>> ToExpression()
    {
        return rating => rating.VideoId == videoId;
    }
}

/// <summary>
/// Specification that matches promoted published videos assigned to a specific promotion spot
/// via their associated <see cref="PromotionLevelEntity.SpotPriority" />.
/// </summary>
public class VideoBySpotPrioritySpecification(int spotPriority) : Specification<VideoEntity>
{
    /// <inheritdoc />
    public override Expression<Func<VideoEntity, bool>> ToExpression()
    {
        DateTimeOffset now = DateTimeOffset.UtcNow;
        return video =>
            video.IsPromoted
            && video.Status == EnumContentStatus.Published
            && (video.PromotedUntil == null || video.PromotedUntil > now)
            && video.PromotionLevel != null
            && video.PromotionLevel.SpotPriority == spotPriority;
    }
}

/// <summary>
/// Specification that matches published free videos — videos with no associated customer
/// (organic uploads) and a <c>Status</c> of <c>Published</c>.
/// Used as fallback content for empty promotion spots and the free video strip on the homepage.
/// </summary>
public class FreeVideoSpecification : Specification<VideoEntity>
{
    /// <inheritdoc />
    public override Expression<Func<VideoEntity, bool>> ToExpression()
    {
        return video => video.Status == EnumContentStatus.Published && video.CustomerId == null;
    }
}

/// <summary>
/// Specification that matches all video ratings belonging to a specific user.
/// </summary>
public class VideoRatingByUserIdSpecification(Guid userId) : Specification<VideoRatingEntity>
{
    /// <inheritdoc />
    public override Expression<Func<VideoRatingEntity, bool>> ToExpression()
    {
        return rating => rating.UserId == userId;
    }
}

/// <summary>
/// Specification that matches all video shares belonging to a specific user.
/// </summary>
public class VideoShareByUserIdSpecification(Guid userId) : Specification<VideoShareEntity>
{
    /// <inheritdoc />
    public override Expression<Func<VideoShareEntity, bool>> ToExpression()
    {
        return share => share.UserId == userId;
    }
}
