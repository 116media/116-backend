using _116.Content.Application.Shared.DTOs;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Lookup.UseCases.Public.Queries.GetAllContentTypes;

/// <summary>
/// Query for retrieving all content types visible to the public.
/// </summary>
public record PublicGetAllContentTypesQuery : IQuery<PublicGetAllContentTypesResult>;

/// <summary>
/// Result of the <see cref="PublicGetAllContentTypesQuery" /> containing all content types.
/// </summary>
/// <param name="ContentTypes">The list of all content types.</param>
public record PublicGetAllContentTypesResult(IReadOnlyList<ContentTypeDto> ContentTypes);
