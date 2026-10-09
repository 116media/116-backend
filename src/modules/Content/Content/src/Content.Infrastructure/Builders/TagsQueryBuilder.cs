using System.Linq.Expressions;
using _116.BuildingBlocks.Domain.Specifications;
using _116.BuildingBlocks.Infrastructure.Extensions;
using _116.Content.Application.Lookup.Specifications;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Constants;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Content.Infrastructure.Builders.Contracts;
using _116.Content.Infrastructure.Persistence;

namespace _116.Content.Infrastructure.Builders;

/// <summary>
/// Builder for constructing tag listing queries. Serves both the alphabetical vocabulary and
/// the most-used ranking, which differ only in their ordering, and projects the per-kind
/// content counts as correlated subqueries of the same statement so a page of tags never
/// becomes a query per tag.
/// </summary>
/// <remarks>
/// Usage:
/// <code>
/// IQueryable&lt;TagCountRow&gt; query = new TagsQueryBuilder()
///     .WithContentType(EnumCoreContentType.Article)
///     .WithOrder(EnumTagOrder.Usage)
///     .WithPublishedOnly(true)
///     .WithLimit(10)
///     .Build(context);
/// </code>
/// </remarks>
public class TagsQueryBuilder : ITagsQueryBuilder
{
    private EnumCoreContentType? _contentType;
    private int? _limit;
    private EnumTagOrder _order = EnumTagOrder.Name;
    private bool _publishedOnly;
    private string? _search;

    /// <inheritdoc />
    public ITagsQueryBuilder WithSearch(string? search)
    {
        _search = search;
        return this;
    }

    /// <inheritdoc />
    public ITagsQueryBuilder WithContentType(EnumCoreContentType? contentType)
    {
        _contentType = contentType;
        return this;
    }

    /// <inheritdoc />
    public ITagsQueryBuilder WithLimit(int? limit)
    {
        _limit = limit;
        return this;
    }

    /// <inheritdoc />
    public ITagsQueryBuilder WithPublishedOnly(bool publishedOnly)
    {
        _publishedOnly = publishedOnly;
        return this;
    }

    /// <inheritdoc />
    public ITagsQueryBuilder WithOrder(EnumTagOrder order)
    {
        _order = order;
        return this;
    }

    /// <inheritdoc />
    public IQueryable<TagCountRow> Build(ContentDbContext context)
    {
        IQueryable<TagEntity> tags = string.IsNullOrWhiteSpace(_search)
            ? context.Tags
            : context.Tags.ApplySpecification(new TagSearchSpecification(search: _search));

        tags = UsedBy(tags: tags, context: context);

        // Ordering runs on the tag, before the counts are projected: a database cannot order by
        // a member of a constructed row.
        tags =
            _order == EnumTagOrder.Usage
                ? tags.OrderByDescending(Usage(context: context)).ThenBy(tag => tag.Name)
                : tags.OrderBy(tag => tag.Name);

        int limit = Math.Min(_limit ?? ContentConstants.MaxReferenceListSize, ContentConstants.MaxReferenceListSize);

        return WithCounts(tags: tags.Take(limit), context: context);
    }

    /// <summary>
    /// Restricts the vocabulary to the tags a content kind actually uses. `Short` and `Custom`
    /// carry no tag associations at all, so asking for them yields nothing rather than everything.
    /// </summary>
    private IQueryable<TagEntity> UsedBy(IQueryable<TagEntity> tags, ContentDbContext context)
    {
        Specification<TagEntity>? specification = _contentType switch
        {
            null => null,
            EnumCoreContentType.Article => new TagUsedByArticlesSpecification(articleTags: context.ArticleTags),
            EnumCoreContentType.Video => new TagUsedByVideosSpecification(videoTags: context.VideoTags),
            EnumCoreContentType.Lyrics => new TagUsedByLyricsSpecification(lyricsTags: context.LyricsTags),
            _ => new TagUsedByNothingSpecification(),
        };

        return specification is null ? tags : tags.ApplySpecification(specification: specification);
    }

    /// <summary>
    /// The usage the ranking measures: the requested content kind alone, or the three kinds
    /// together, counted published-only or across every association.
    /// </summary>
    private Expression<Func<TagEntity, int>> Usage(ContentDbContext context)
    {
        if (!_publishedOnly)
        {
            return _contentType switch
            {
                EnumCoreContentType.Article => tag => context.ArticleTags.Count(at => at.TagId == tag.Id),
                EnumCoreContentType.Video => tag => context.VideoTags.Count(vt => vt.TagId == tag.Id),
                EnumCoreContentType.Lyrics => tag => context.LyricsTags.Count(lt => lt.TagId == tag.Id),
                _ => tag =>
                    context.ArticleTags.Count(at => at.TagId == tag.Id)
                    + context.VideoTags.Count(vt => vt.TagId == tag.Id)
                    + context.LyricsTags.Count(lt => lt.TagId == tag.Id),
            };
        }

        return _contentType switch
        {
            EnumCoreContentType.Article => tag =>
                context.Articles.Count(article =>
                    article.Status == EnumContentStatus.Published && article.Tags.Any(at => at.TagId == tag.Id)
                ),
            EnumCoreContentType.Video => tag =>
                context.Videos.Count(video =>
                    video.Status == EnumContentStatus.Published && video.Tags.Any(vt => vt.TagId == tag.Id)
                ),
            EnumCoreContentType.Lyrics => tag =>
                context.Lyrics.Count(lyrics =>
                    lyrics.Status == EnumContentStatus.Published && lyrics.Tags.Any(lt => lt.TagId == tag.Id)
                ),
            _ => tag =>
                context.Articles.Count(article =>
                    article.Status == EnumContentStatus.Published && article.Tags.Any(at => at.TagId == tag.Id)
                )
                + context.Videos.Count(video =>
                    video.Status == EnumContentStatus.Published && video.Tags.Any(vt => vt.TagId == tag.Id)
                )
                + context.Lyrics.Count(lyrics =>
                    lyrics.Status == EnumContentStatus.Published && lyrics.Tags.Any(lt => lt.TagId == tag.Id)
                ),
        };
    }

    /// <summary>
    /// Counts the content of each kind that carries the tag, published only or every
    /// association, as correlated subqueries of the same statement.
    /// </summary>
    private IQueryable<TagCountRow> WithCounts(IQueryable<TagEntity> tags, ContentDbContext context)
    {
        return _publishedOnly
            ? tags.Select(tag => new TagCountRow(
                tag,
                context.Articles.Count(article =>
                    article.Status == EnumContentStatus.Published && article.Tags.Any(at => at.TagId == tag.Id)
                ),
                context.Videos.Count(video =>
                    video.Status == EnumContentStatus.Published && video.Tags.Any(vt => vt.TagId == tag.Id)
                ),
                context.Lyrics.Count(lyrics =>
                    lyrics.Status == EnumContentStatus.Published && lyrics.Tags.Any(lt => lt.TagId == tag.Id)
                )
            ))
            : tags.Select(tag => new TagCountRow(
                tag,
                context.ArticleTags.Count(at => at.TagId == tag.Id),
                context.VideoTags.Count(vt => vt.TagId == tag.Id),
                context.LyricsTags.Count(lt => lt.TagId == tag.Id)
            ));
    }
}
