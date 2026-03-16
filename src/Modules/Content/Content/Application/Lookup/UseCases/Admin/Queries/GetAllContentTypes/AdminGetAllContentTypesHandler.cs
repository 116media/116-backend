using _116.Content.Application.Shared.DTOs;
using _116.Content.Application.Shared.Mappers;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Shared.Contracts.Application.CQRS;
using MapsterMapper;

namespace _116.Content.Application.Lookup.UseCases.Admin.Queries.GetAllContentTypes;

/// <summary>
/// Handles the <see cref="AdminGetAllContentTypesQuery" /> to retrieve all content types.
/// </summary>
/// <param name="lookupRepository">Repository for lookup data access operations.</param>
/// <param name="mapper">Mapster mapper for entity-to-DTO transformations.</param>
public class AdminGetAllContentTypesHandler(ILookupRepository lookupRepository, IMapper mapper)
    : IQueryHandler<AdminGetAllContentTypesQuery, AdminGetAllContentTypesResult>
{
    /// <inheritdoc />
    public async Task<AdminGetAllContentTypesResult> Handle(
        AdminGetAllContentTypesQuery query,
        CancellationToken cancellationToken
    )
    {
        IReadOnlyList<ContentTypeEntity> contentTypes = await lookupRepository.GetAllContentTypesAsync(
            cancellationToken: cancellationToken
        );

        IReadOnlyList<ContentTypeDto> dtoList = contentTypes.ToContentTypeDtos(mapper);
        return new AdminGetAllContentTypesResult(ContentTypes: dtoList);
    }
}
