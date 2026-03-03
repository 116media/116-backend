using _116.Content.Application.Shared.DTOs;
using _116.Content.Application.Shared.Mappers;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Shared.Contracts.Application.CQRS;
using MapsterMapper;

namespace _116.Content.Application.Lookup.UseCases.Public.Queries.GetAllTags;

/// <summary>
/// Handles the <see cref="GetAllTagsQuery" /> to retrieve all tags.
/// </summary>
/// <param name="lookupRepository">Repository for lookup data access operations.</param>
/// <param name="mapper">Mapster mapper for entity-to-DTO transformations.</param>
public class GetAllTagsHandler(ILookupRepository lookupRepository, IMapper mapper)
    : IQueryHandler<GetAllTagsQuery, GetAllTagsResult>
{
    /// <inheritdoc />
    public async Task<GetAllTagsResult> Handle(GetAllTagsQuery query, CancellationToken cancellationToken)
    {
        IReadOnlyList<TagEntity> tags = await lookupRepository.GetAllTagsAsync(
            search: query.Search,
            cancellationToken: cancellationToken
        );

        IReadOnlyList<TagDto> dtoList = tags.ToTagDtos(mapper);
        return new GetAllTagsResult(Tags: dtoList);
    }
}
