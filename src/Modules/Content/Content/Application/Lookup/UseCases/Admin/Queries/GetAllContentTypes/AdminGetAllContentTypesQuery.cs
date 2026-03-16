using _116.Content.Application.Shared.DTOs;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Lookup.UseCases.Admin.Queries.GetAllContentTypes;

/// <summary>
/// Query for retrieving all content types.
/// </summary>
public record AdminGetAllContentTypesQuery : IQuery<AdminGetAllContentTypesResult>;

/// <summary>
/// Result of the <see cref="AdminGetAllContentTypesQuery" /> containing all content types.
/// </summary>
/// <param name="ContentTypes">The list of all content types.</param>
public record AdminGetAllContentTypesResult(IReadOnlyList<ContentTypeDto> ContentTypes);
