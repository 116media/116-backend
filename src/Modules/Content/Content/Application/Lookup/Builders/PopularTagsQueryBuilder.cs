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
        // Count article usages per tag (hits ix_article_tags_tag_id)
        var articleCounts = context.ArticleTags.GroupBy(at => at.TagId);

        return context
            .Tags.GroupJoin(articleCounts, tag => tag.Id, grp => grp.Key, (tag, articleGrp) => new { tag, articleGrp })
            .SelectMany(
                x => x.articleGrp.DefaultIfEmpty(),
                (x, articleGrp) => new { x.tag, totalCount = articleGrp == null ? 0 : articleGrp.Count() }
            )
            .OrderByDescending(x => x.totalCount)
            .ThenBy(x => x.tag.Name)
            .Select(x => x.tag);
    }

    private static IQueryable<TagEntity> BuildVideoQuery(ContentDbContext context)
    {
        // Count video usages per tag (hits ix_video_tags_tag_id)
        var videoCounts = context.VideoTags.GroupBy(vt => vt.TagId);

        return context
            .Tags.GroupJoin(videoCounts, tag => tag.Id, grp => grp.Key, (tag, videoGrp) => new { tag, videoGrp })
            .SelectMany(
                x => x.videoGrp.DefaultIfEmpty(),
                (x, videoGrp) => new { x.tag, totalCount = videoGrp == null ? 0 : videoGrp.Count() }
            )
            .OrderByDescending(x => x.totalCount)
            .ThenBy(x => x.tag.Name)
            .Select(x => x.tag);
    }

    private static IQueryable<TagEntity> BuildCombinedQuery(ContentDbContext context)
    {
        // Count both article and video usages per tag
        var articleCounts = context.ArticleTags.GroupBy(at => at.TagId);
        var videoCounts = context.VideoTags.GroupBy(vt => vt.TagId);

        return context
            .Tags.GroupJoin(articleCounts, tag => tag.Id, grp => grp.Key, (tag, articleGrp) => new { tag, articleGrp })
            .SelectMany(
                x => x.articleGrp.DefaultIfEmpty(),
                (x, articleGrp) => new { x.tag, articleCount = articleGrp == null ? 0 : articleGrp.Count() }
            )
            .GroupJoin(
                videoCounts,
                x => x.tag.Id,
                grp => grp.Key,
                (x, videoGrp) =>
                    new
                    {
                        x.tag,
                        x.articleCount,
                        videoGrp,
                    }
            )
            .SelectMany(
                x => x.videoGrp.DefaultIfEmpty(),
                (x, videoGrp) => new { x.tag, totalCount = x.articleCount + (videoGrp == null ? 0 : videoGrp.Count()) }
            )
            .OrderByDescending(x => x.totalCount)
            .ThenBy(x => x.tag.Name)
            .Select(x => x.tag);
    }
}
