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
public class AbandonedDraftSpecification(DateTimeOffset cutoff) : Specification<ArticleEntity>
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
