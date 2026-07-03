using _116.Content.Application.Shared.DTOs;
using _116.Content.Domain.Enums;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Lookup.UseCases.Public.Queries.GetAllTags;

/// <summary>
/// Query for retrieving all tags visible to the public.
/// </summary>
/// <param name="Search">Optional search term to filter tags by name or slug (case-insensitive, partial match).</param>
/// <param name="ContentType">
/// Optional content type filter. Pass <see cref="EnumCoreContentType.Article"/> to return only
/// tags that have at least one article association, <see cref="EnumCoreContentType.Video"/> to
/// return only tags that have at least one video association. When <see langword="null" />, all
/// tags are returned regardless of content-type usage.
/// </param>
/// <param name="Limit">
/// Maximum number of tags to return. When <see langword="null" /> all matching tags are
/// returned ordered by name. Defaults to <see langword="null" />.
/// </param>
public record PublicGetAllTagsQuery(string? Search = null, EnumCoreContentType? ContentType = null, int? Limit = null)
    : IQuery<PublicGetAllTagsResult>;

/// <summary>
/// Result of the <see cref="PublicGetAllTagsQuery" /> containing all tags.
/// </summary>
/// <param name="Tags">The list of all tags.</param>
public record PublicGetAllTagsResult(IReadOnlyList<TagDto> Tags);
