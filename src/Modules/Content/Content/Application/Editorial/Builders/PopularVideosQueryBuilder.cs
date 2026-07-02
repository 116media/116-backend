using _116.Content.Application.Editorial.Builders.Contracts;
using _116.Content.Application.Editorial.Constants;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Content.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace _116.Content.Application.Editorial.Builders;

/// <summary>
/// Builder for constructing the popular-videos query. Ranks published videos by a weighted
/// engagement score computed in the database <c>ORDER BY</c>, tie-broken by publish date
/// descending.
/// </summary>
/// <remarks>
/// Usage:
/// <code>
/// IQueryable&lt;VideoEntity&gt; query = new PopularVideosQueryBuilder()
///     .WithCategory(categoryId)
///     .WithExcludeId(excludeId)
///     .WithLimit(5)
///     .Build(context);
/// </code>
/// The score weights come from <see cref="PopularVideosScoring" /> so the ranking is tunable
/// in one place. The rating-volume term <c>RatingAverage * RatingCount</c> is cast to
/// <see langword="double" /> so the whole score is evaluated in floating point: leaving it in
/// the narrow <c>numeric</c> precision of <c>RatingAverage</c> makes PostgreSQL raise a numeric
/// field overflow as soon as the score exceeds that column's range.
/// </remarks>
public class PopularVideosQueryBuilder : IPopularVideosQueryBuilder
{
    private Guid? _categoryId;
    private Guid? _excludeId;
    private int? _limit;

    /// <inheritdoc />
    public IPopularVideosQueryBuilder WithCategory(Guid? categoryId)
    {
        _categoryId = categoryId;
        return this;
    }

    /// <inheritdoc />
    public IPopularVideosQueryBuilder WithExcludeId(Guid? excludeId)
    {
        _excludeId = excludeId;
        return this;
    }

    /// <inheritdoc />
    public IPopularVideosQueryBuilder WithLimit(int? limit)
    {
        _limit = limit;
        return this;
    }

    /// <inheritdoc />
    public IQueryable<VideoEntity> Build(ContentDbContext context)
    {
        const int ratingWeight = PopularVideosScoring.RatingWeight;
        const int shareWeight = PopularVideosScoring.ShareWeight;

        IQueryable<VideoEntity> query = context
            .Videos.Include(v => v.Category)
            .Where(v => v.Status == EnumContentStatus.Published);

        if (_categoryId.HasValue)
        {
            query = query.Where(v => v.CategoryId == _categoryId.Value);
        }

        if (_excludeId.HasValue)
        {
            query = query.Where(v => v.Id != _excludeId.Value);
        }

        query = query
            .OrderByDescending(v =>
                (ratingWeight * ((double)v.RatingAverage * v.RatingCount)) + (shareWeight * v.ShareCount)
            )
            .ThenByDescending(v => v.PublishedAt);

        if (_limit.HasValue)
        {
            query = query.Take(_limit.Value);
        }

        return query;
    }
}
