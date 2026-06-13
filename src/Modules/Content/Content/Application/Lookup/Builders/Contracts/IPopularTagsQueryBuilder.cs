using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Content.Infrastructure.Persistence;

namespace _116.Content.Application.Lookup.Builders.Contracts;

/// <summary>
/// Interface for building popular-tags queries using the builder pattern.
/// Encapsulates the join logic that counts tag usage across article and/or video associations.
/// </summary>
public interface IPopularTagsQueryBuilder
{
    /// <summary>
    /// Scopes tag popularity to a specific content type.
    /// Pass <see cref="EnumCoreContentType.Article"/> to rank by article usage only,
    /// <see cref="EnumCoreContentType.Video"/> to rank by video usage only.
    /// When not called, popularity is ranked by combined usage across all content types.
    /// </summary>
    IPopularTagsQueryBuilder WithContentType(EnumCoreContentType? contentType);

    /// <summary>
    /// Limits the number of tags returned.
    /// When not called, all tags ordered by popularity are returned.
    /// </summary>
    IPopularTagsQueryBuilder WithLimit(int? limit);

    /// <summary>
    /// Builds and returns the final ordered, optionally limited <see cref="IQueryable" />.
    /// </summary>
    IQueryable<TagEntity> Build(ContentDbContext context);
}
