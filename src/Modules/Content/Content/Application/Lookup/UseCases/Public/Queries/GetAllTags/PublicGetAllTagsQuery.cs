using _116.Content.Application.Shared.DTOs;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Lookup.UseCases.Public.Queries.GetAllTags;

/// <summary>
/// Query for retrieving all tags visible to the public.
/// </summary>
/// <param name="Search">Optional search term to filter tags by name or slug (case-insensitive, partial match).</param>
public record PublicGetAllTagsQuery(string? Search = null) : IQuery<PublicGetAllTagsResult>;

/// <summary>
/// Result of the <see cref="PublicGetAllTagsQuery" /> containing all tags.
/// </summary>
/// <param name="Tags">The list of all tags.</param>
public record PublicGetAllTagsResult(IReadOnlyList<TagDto> Tags);
