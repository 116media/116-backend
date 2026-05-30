using _116.Content.Application.Shared.DTOs;
using _116.Content.Application.Shared.Mappers;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Shared.Contracts.Application.CQRS;
using MapsterMapper;

namespace _116.Content.Application.Lookup.UseCases.Public.Queries.GetAllContentTypes;

/// <summary>
/// Handles the <see cref="PublicGetAllContentTypesQuery" /> to retrieve all content types.
/// </summary>
/// <param name="lookupRepository">Repository for lookup data access operations.</param>
/// <param name="mapper">Mapster mapper for entity-to-DTO transformations.</param>
public class PublicGetAllContentTypesHandler(ILookupRepository lookupRepository, IMapper mapper)
    : IQueryHandler<PublicGetAllContentTypesQuery, PublicGetAllContentTypesResult>
{
    /// <inheritdoc />
    public async Task<PublicGetAllContentTypesResult> Handle(
        PublicGetAllContentTypesQuery query,
        CancellationToken cancellationToken
    )
    {
        IReadOnlyList<ContentTypeEntity> contentTypes = await lookupRepository.GetActiveContentTypesAsync(
            cancellationToken: cancellationToken
        );

        IReadOnlyList<ContentTypeDto> dtoList = contentTypes.ToContentTypeDtos(mapper);
        return new PublicGetAllContentTypesResult(ContentTypes: dtoList);
    }
}
