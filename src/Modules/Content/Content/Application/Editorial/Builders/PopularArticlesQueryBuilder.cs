using _116.Content.Application.Editorial.Builders.Contracts;
using _116.Content.Application.Editorial.Constants;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Content.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace _116.Content.Application.Editorial.Builders;

/// <summary>
/// Builder for constructing the popular-articles query. Ranks published articles by a
/// weighted engagement score computed in the database <c>ORDER BY</c>, tie-broken by
/// publish date descending.
/// </summary>
/// <remarks>
/// Usage:
/// <code>
/// IQueryable&lt;ArticleEntity&gt; query = new PopularArticlesQueryBuilder()
///     .WithCategory(categoryId)
///     .WithExcludeId(excludeId)
///     .WithLimit(5)
///     .Build(context);
/// </code>
/// The score weights come from <see cref="PopularArticlesScoring" /> so the ranking is
/// tunable in one place. All four counters are non-negative integers, so the score is an
/// integer expression PostgreSQL evaluates directly in the <c>ORDER BY</c>.
/// </remarks>
public class PopularArticlesQueryBuilder : IPopularArticlesQueryBuilder
{
    private Guid? _categoryId;
    private Guid? _excludeId;
    private int? _limit;

    /// <inheritdoc />
    public IPopularArticlesQueryBuilder WithCategory(Guid? categoryId)
    {
        _categoryId = categoryId;
        return this;
    }

    /// <inheritdoc />
    public IPopularArticlesQueryBuilder WithExcludeId(Guid? excludeId)
    {
        _excludeId = excludeId;
        return this;
    }

    /// <inheritdoc />
    public IPopularArticlesQueryBuilder WithLimit(int? limit)
    {
        _limit = limit;
        return this;
    }

    /// <inheritdoc />
    public IQueryable<ArticleEntity> Build(ContentDbContext context)
    {
        const int likeWeight = PopularArticlesScoring.LikeWeight;
        const int commentWeight = PopularArticlesScoring.CommentWeight;
        const int shareWeight = PopularArticlesScoring.ShareWeight;
        const int bookmarkWeight = PopularArticlesScoring.BookmarkWeight;

        IQueryable<ArticleEntity> query = context
            .Articles.Include(a => a.Category)
            .Where(a => a.Status == EnumContentStatus.Published);

        if (_categoryId.HasValue)
        {
            query = query.Where(a => a.CategoryId == _categoryId.Value);
        }

        if (_excludeId.HasValue)
        {
            query = query.Where(a => a.Id != _excludeId.Value);
        }

        query = query
            .OrderByDescending(a =>
                (likeWeight * a.LikeCount)
                + (commentWeight * a.CommentCount)
                + (shareWeight * a.ShareCount)
                + (bookmarkWeight * a.BookmarkCount)
            )
            .ThenByDescending(a => a.PublishedAt);

        if (_limit.HasValue)
        {
            query = query.Take(_limit.Value);
        }

        return query;
    }
}
