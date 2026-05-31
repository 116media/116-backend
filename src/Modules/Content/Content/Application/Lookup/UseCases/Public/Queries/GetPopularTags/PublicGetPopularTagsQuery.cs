using _116.Content.Application.Shared.DTOs;
using _116.Content.Domain.Enums;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Lookup.UseCases.Public.Queries.GetPopularTags;

/// <summary>
/// Query for retrieving the most-used tags, optionally scoped to a content type.
/// </summary>
/// <param name="Limit">
/// Maximum number of tags to return. When <see langword="null" /> all tags are
/// returned ordered by popularity. Defaults to <see langword="null" />.
/// </param>
/// <param name="ContentType">
/// Optional content type filter. Pass <see cref="EnumCoreContentType.Article"/> to rank
/// by article usage only, <see cref="EnumCoreContentType.Video"/> to rank by video usage
/// only. When <see langword="null" />, popularity is ranked by combined usage across all
/// content types.
/// </param>
public record PublicGetPopularTagsQuery(int? Limit = null, EnumCoreContentType? ContentType = null)
    : IQuery<PublicGetPopularTagsResult>;

/// <summary>
/// Result of the <see cref="PublicGetPopularTagsQuery" /> containing the most popular tags.
/// </summary>
/// <param name="Tags">The list of popular tags ordered by usage count descending.</param>
public record PublicGetPopularTagsResult(IReadOnlyList<TagDto> Tags);
