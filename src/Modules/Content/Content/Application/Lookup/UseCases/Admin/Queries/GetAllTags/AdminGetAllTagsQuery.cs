using _116.Content.Application.Shared.DTOs;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Lookup.UseCases.Admin.Queries.GetAllTags;

/// <summary>
/// Query for retrieving all tags for admin management.
/// </summary>
/// <param name="Search">
/// Optional search term to filter tags by name or slug (case-insensitive, partial match).
/// </param>
public record AdminGetAllTagsQuery(string? Search = null) : IQuery<AdminGetAllTagsResult>;

/// <summary>
/// Result of the <see cref="AdminGetAllTagsQuery" /> containing all tags.
/// </summary>
/// <param name="Tags">The list of all tags.</param>
public record AdminGetAllTagsResult(IReadOnlyList<TagDto> Tags);
