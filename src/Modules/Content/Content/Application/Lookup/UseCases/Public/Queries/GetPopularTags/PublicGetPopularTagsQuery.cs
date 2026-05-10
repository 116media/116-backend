using _116.Content.Application.Shared.DTOs;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Lookup.UseCases.Public.Queries.GetPopularTags;

/// <summary>
/// Query for retrieving the most-used tags across articles and videos.
/// </summary>
/// <param name="Limit">
/// Maximum number of tags to return. When <see langword="null" /> all tags are
/// returned ordered by popularity. Defaults to <see langword="null" />.
/// </param>
public record PublicGetPopularTagsQuery(int? Limit = null) : IQuery<PublicGetPopularTagsResult>;

/// <summary>
/// Result of the <see cref="PublicGetPopularTagsQuery" /> containing the most popular tags.
/// </summary>
/// <param name="Tags">The list of popular tags ordered by usage count descending.</param>
public record PublicGetPopularTagsResult(IReadOnlyList<TagDto> Tags);
