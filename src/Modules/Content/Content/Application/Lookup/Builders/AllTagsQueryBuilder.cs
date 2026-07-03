using _116.Content.Application.Lookup.Builders.Contracts;
using _116.Content.Application.Lookup.Specifications;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Content.Infrastructure.Persistence;
using _116.Shared.Infrastructure.Extensions;

namespace _116.Content.Application.Lookup.Builders;

/// <summary>
/// Builder for constructing all-tags queries.
/// Implements the Builder pattern to eliminate branching in the repository method.
/// </summary>
/// <remarks>
/// Usage:
/// <code>
/// IQueryable&lt;TagEntity&gt; query = new AllTagsQueryBuilder()
///     .WithSearch("fally")
///     .WithContentType(EnumCoreContentType.Article)
///     .WithLimit(50)
///     .Build(context);
/// </code>
/// </remarks>
public class AllTagsQueryBuilder : IAllTagsQueryBuilder
{
    private string? _search;
    private EnumCoreContentType? _contentType;
    private int? _limit;

    /// <inheritdoc />
    public IAllTagsQueryBuilder WithSearch(string? search)
    {
        _search = search;
        return this;
    }

    /// <inheritdoc />
    public IAllTagsQueryBuilder WithContentType(EnumCoreContentType? contentType)
    {
        _contentType = contentType;
        return this;
    }

    /// <inheritdoc />
    public IAllTagsQueryBuilder WithLimit(int? limit)
    {
        _limit = limit;
        return this;
    }

    /// <inheritdoc />
    public IQueryable<TagEntity> Build(ContentDbContext context)
    {
        IQueryable<TagEntity> query = string.IsNullOrWhiteSpace(_search)
            ? context.Tags
            : context.Tags.ApplySpecification(new TagSearchSpecification(search: _search));

        query = _contentType switch
        {
            EnumCoreContentType.Article => query.Where(tag => context.ArticleTags.Any(at => at.TagId == tag.Id)),
            EnumCoreContentType.Video => query.Where(tag => context.VideoTags.Any(vt => vt.TagId == tag.Id)),
            _ => query,
        };

        query = query.OrderBy(tag => tag.Name);

        if (_limit.HasValue)
        {
            query = query.Take(_limit.Value);
        }

        return query;
    }
}
