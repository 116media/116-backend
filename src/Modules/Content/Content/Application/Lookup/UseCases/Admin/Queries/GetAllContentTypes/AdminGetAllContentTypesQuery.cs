using _116.Content.Application.Shared.DTOs;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Lookup.UseCases.Admin.Queries.GetAllContentTypes;

/// <summary>
/// Query for retrieving all content types.
/// </summary>
/// <param name="Search">
/// Optional search term to filter content types by name (case-insensitive, partial match).
/// </param>
public record AdminGetAllContentTypesQuery(string? Search = null) : IQuery<AdminGetAllContentTypesResult>;

/// <summary>
/// Result of the <see cref="AdminGetAllContentTypesQuery" /> containing all content types.
/// </summary>
/// <param name="ContentTypes">The list of all content types.</param>
public record AdminGetAllContentTypesResult(IReadOnlyList<ContentTypeDto> ContentTypes);
