using _116.Content.Application.Shared.DTOs;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Shared.Contracts.Application.CQRS;
using Mapster;

namespace _116.Content.Application.Lookup.UseCases.Admin.Queries.GetAllContentTypes;

/// <summary>
/// Handles the <see cref="GetAllContentTypesQuery" /> to retrieve all content types.
/// </summary>
/// <param name="lookupRepository">Repository for lookup data access operations.</param>
public class GetAllContentTypesHandler(ILookupRepository lookupRepository)
    : IQueryHandler<GetAllContentTypesQuery, GetAllContentTypesResult>
{
    /// <inheritdoc />
    public async Task<GetAllContentTypesResult> Handle(
        GetAllContentTypesQuery query,
        CancellationToken cancellationToken
    )
    {
        IReadOnlyList<ContentTypeEntity> contentTypes = await lookupRepository.GetAllContentTypesAsync(
            cancellationToken: cancellationToken
        );

        var dtos = contentTypes.Adapt<IReadOnlyList<ContentTypeDto>>();

        return new GetAllContentTypesResult(ContentTypes: dtos);
    }
}
