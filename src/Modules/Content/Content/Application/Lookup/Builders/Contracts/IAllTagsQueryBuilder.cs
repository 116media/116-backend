using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Content.Infrastructure.Persistence;

namespace _116.Content.Application.Lookup.Builders.Contracts;

/// <summary>
/// Interface for building all-tags queries using the builder pattern.
/// Encapsulates the optional search filter and the content-type association filter
/// that narrows the tag vocabulary to tags actually used by a given content type.
/// </summary>
public interface IAllTagsQueryBuilder
{
    /// <summary>
    /// Filters tags by a case-insensitive partial match on Name and Slug.
    /// When not called, or when the search term is blank, no search filter is applied.
    /// </summary>
    IAllTagsQueryBuilder WithSearch(string? search);

    /// <summary>
    /// Restricts the result to tags that are associated with the given content type.
    /// Pass <see cref="EnumCoreContentType.Article"/> to return only tags that have at
    /// least one article association, <see cref="EnumCoreContentType.Video"/> to return
    /// only tags that have at least one video association.
    /// When not called, tags are returned regardless of content-type usage.
    /// </summary>
    IAllTagsQueryBuilder WithContentType(EnumCoreContentType? contentType);

    /// <summary>
    /// Limits the number of tags returned.
    /// When not called, all tags ordered by name are returned.
    /// </summary>
    IAllTagsQueryBuilder WithLimit(int? limit);

    /// <summary>
    /// Builds and returns the final ordered, optionally limited <see cref="IQueryable" />.
    /// </summary>
    IQueryable<TagEntity> Build(ContentDbContext context);
}
