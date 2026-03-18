using System.Linq.Expressions;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Shared.Application.Specifications;
using Microsoft.EntityFrameworkCore;

namespace _116.Content.Application.Editorial.Specifications;

/// <summary>
/// Specification that matches an article by its unique identifier.
/// </summary>
public class ArticleByIdSpecification(Guid id) : Specification<ArticleEntity>
{
    /// <inheritdoc />
    public override Expression<Func<ArticleEntity, bool>> ToExpression()
    {
        return article => article.Id == id;
    }
}

/// <summary>
/// Specification that matches an article by its URL-safe slug (case-insensitive).
/// </summary>
public class ArticleBySlugSpecification(string slug) : Specification<ArticleEntity>
{
    /// <inheritdoc />
    public override Expression<Func<ArticleEntity, bool>> ToExpression()
    {
        return article => EF.Functions.ILike(article.Slug, slug);
    }
}

/// <summary>
/// Specification that matches articles by their content status.
/// </summary>
public class ArticleByStatusSpecification(EnumContentStatus status) : Specification<ArticleEntity>
{
    /// <inheritdoc />
    public override Expression<Func<ArticleEntity, bool>> ToExpression()
    {
        return article => article.Status == status;
    }
}

/// <summary>
/// Specification that matches articles belonging to a specific category.
/// </summary>
public class ArticleByCategorySpecification(Guid categoryId) : Specification<ArticleEntity>
{
    /// <inheritdoc />
    public override Expression<Func<ArticleEntity, bool>> ToExpression()
    {
        return article => article.CategoryId == categoryId;
    }
}

/// <summary>
/// Specification for full-text search across article Title, Headline, Body,
/// MetaTitle, and MetaDescription fields.
/// Uses case-insensitive matching (ILIKE in PostgreSQL).
/// </summary>
public class ArticleSearchSpecification(string search) : Specification<ArticleEntity>
{
    /// <inheritdoc />
    public override Expression<Func<ArticleEntity, bool>> ToExpression()
    {
        string pattern = $"%{search}%";
        return article =>
            EF.Functions.ILike(article.Title, pattern)
            || EF.Functions.ILike(article.Headline, pattern)
            || EF.Functions.ILike(article.Body, pattern)
            || (article.MetaTitle != null && EF.Functions.ILike(article.MetaTitle, pattern))
            || (article.MetaDescription != null && EF.Functions.ILike(article.MetaDescription, pattern));
    }
}

/// <summary>
/// Specification that matches articles that are currently featured and published.
/// Featured articles must have <c>IsFeatured = true</c>, a future or null <c>FeaturedUntil</c>,
/// and <c>Status = Published</c>.
/// </summary>
public class FeaturedArticleSpecification : Specification<ArticleEntity>
{
    /// <inheritdoc />
    public override Expression<Func<ArticleEntity, bool>> ToExpression()
    {
        DateTimeOffset now = DateTimeOffset.UtcNow;
        return article =>
            article.IsFeatured
            && article.Status == EnumContentStatus.Published
            && (article.FeaturedUntil == null || article.FeaturedUntil > now);
    }
}

/// <summary>
/// Specification that matches draft articles with no content created before a given cutoff date.
/// Used by the background abandoned-draft cleanup job.
/// </summary>
public class AbandonedDraftSpecification(DateTime cutoff) : Specification<ArticleEntity>
{
    /// <inheritdoc />
    public override Expression<Func<ArticleEntity, bool>> ToExpression()
    {
        return article =>
            article.Status == EnumContentStatus.Draft
            && article.Body == string.Empty
            && article.Headline == string.Empty
            && article.CreatedAt < cutoff;
    }
}

/// <summary>
/// Specification that matches an article by its associated order item identifier.
/// </summary>
public class ArticleByOrderItemIdSpecification(Guid orderItemId) : Specification<ArticleEntity>
{
    /// <inheritdoc />
    public override Expression<Func<ArticleEntity, bool>> ToExpression()
    {
        return article => article.OrderItemId == orderItemId;
    }
}

/// <summary>
/// Specification that matches article images belonging to a specific article.
/// </summary>
public class ArticleImageByArticleIdSpecification(Guid articleId) : Specification<ArticleImageEntity>
{
    /// <inheritdoc />
    public override Expression<Func<ArticleImageEntity, bool>> ToExpression()
    {
        return image => image.ArticleId == articleId;
    }
}

/// <summary>
/// Specification that matches article tags belonging to a specific article.
/// </summary>
public class ArticleTagByArticleIdSpecification(Guid articleId) : Specification<ArticleTagEntity>
{
    /// <inheritdoc />
    public override Expression<Func<ArticleTagEntity, bool>> ToExpression()
    {
        return tag => tag.ArticleId == articleId;
    }
}

/// <summary>
/// Specification that matches an article like by user and article identifiers.
/// </summary>
public class ArticleLikeByUserAndArticleSpecification(Guid userId, Guid articleId) : Specification<ArticleLikeEntity>
{
    /// <inheritdoc />
    public override Expression<Func<ArticleLikeEntity, bool>> ToExpression()
    {
        return like => like.UserId == userId && like.ArticleId == articleId;
    }
}

/// <summary>
/// Specification that matches an article bookmark by user and article identifiers.
/// </summary>
public class ArticleBookmarkByUserAndArticleSpecification(Guid userId, Guid articleId)
    : Specification<ArticleBookmarkEntity>
{
    /// <inheritdoc />
    public override Expression<Func<ArticleBookmarkEntity, bool>> ToExpression()
    {
        return bookmark => bookmark.UserId == userId && bookmark.ArticleId == articleId;
    }
}

/// <summary>
/// Specification that matches all article bookmarks belonging to a specific user.
/// </summary>
public class ArticleBookmarkByUserIdSpecification(Guid userId) : Specification<ArticleBookmarkEntity>
{
    /// <inheritdoc />
    public override Expression<Func<ArticleBookmarkEntity, bool>> ToExpression()
    {
        return bookmark => bookmark.UserId == userId;
    }
}

/// <summary>
/// Specification that matches article comments belonging to a specific article.
/// </summary>
public class ArticleCommentByArticleIdSpecification(Guid articleId) : Specification<ArticleCommentEntity>
{
    /// <inheritdoc />
    public override Expression<Func<ArticleCommentEntity, bool>> ToExpression()
    {
        return comment => comment.ArticleId == articleId;
    }
}

/// <summary>
/// Specification that matches an article comment by its unique identifier.
/// </summary>
public class ArticleCommentByIdSpecification(Guid commentId) : Specification<ArticleCommentEntity>
{
    /// <inheritdoc />
    public override Expression<Func<ArticleCommentEntity, bool>> ToExpression()
    {
        return comment => comment.Id == commentId;
    }
}
