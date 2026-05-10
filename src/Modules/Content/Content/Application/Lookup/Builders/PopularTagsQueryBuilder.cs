using _116.Content.Application.Lookup.Builders.Contracts;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Content.Infrastructure.Persistence;

namespace _116.Content.Application.Lookup.Builders;

/// <summary>
/// Builder for constructing popular-tags queries.
/// Implements the Builder pattern to eliminate branching in the repository method.
/// </summary>
/// <remarks>
/// Usage:
/// <code>
/// IQueryable&lt;TagEntity&gt; query = new PopularTagsQueryBuilder()
///     .WithContentType(EnumCoreContentType.Article)
///     .WithLimit(10)
///     .Build(context);
/// </code>
/// </remarks>
public class PopularTagsQueryBuilder : IPopularTagsQueryBuilder
{
    private EnumCoreContentType? _contentType;
    private int? _limit;

    /// <inheritdoc />
    public IPopularTagsQueryBuilder WithContentType(EnumCoreContentType? contentType)
    {
        _contentType = contentType;
        return this;
    }

    /// <inheritdoc />
    public IPopularTagsQueryBuilder WithLimit(int? limit)
    {
        _limit = limit;
        return this;
    }

    /// <inheritdoc />
    public IQueryable<TagEntity> Build(ContentDbContext context)
    {
        IQueryable<TagEntity> query = _contentType switch
        {
            EnumCoreContentType.Article => BuildArticleQuery(context),
            EnumCoreContentType.Video => BuildVideoQuery(context),
            _ => BuildCombinedQuery(context),
        };

        if (_limit.HasValue)
        {
            query = query.Take(_limit.Value);
        }

        return query;
    }

    private static IQueryable<TagEntity> BuildArticleQuery(ContentDbContext context)
    {
        return context
            .Tags.Select(tag => new { tag, totalCount = context.ArticleTags.Count(at => at.TagId == tag.Id) })
            .OrderByDescending(x => x.totalCount)
            .ThenBy(x => x.tag.Name)
            .Select(x => x.tag);
    }

    private static IQueryable<TagEntity> BuildVideoQuery(ContentDbContext context)
    {
        return context
            .Tags.Select(tag => new { tag, totalCount = context.VideoTags.Count(vt => vt.TagId == tag.Id) })
            .OrderByDescending(x => x.totalCount)
            .ThenBy(x => x.tag.Name)
            .Select(x => x.tag);
    }

    private static IQueryable<TagEntity> BuildCombinedQuery(ContentDbContext context)
    {
        return context
            .Tags.Select(tag => new
            {
                tag,
                totalCount = context.ArticleTags.Count(at => at.TagId == tag.Id)
                    + context.VideoTags.Count(vt => vt.TagId == tag.Id),
            })
            .OrderByDescending(x => x.totalCount)
            .ThenBy(x => x.tag.Name)
            .Select(x => x.tag);
    }
}
