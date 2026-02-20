using _116.Content.Application.Shared.DTOs;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Shared.Contracts.Application.CQRS;
using Mapster;

namespace _116.Content.Application.Lookup.UseCases.Public.Queries.GetAllTags;

/// <summary>
/// Handles the <see cref="GetAllTagsQuery" /> to retrieve all tags.
/// </summary>
/// <param name="lookupRepository">Repository for lookup data access operations.</param>
public class GetAllTagsHandler(ILookupRepository lookupRepository) : IQueryHandler<GetAllTagsQuery, GetAllTagsResult>
{
    /// <inheritdoc />
    public async Task<GetAllTagsResult> Handle(GetAllTagsQuery query, CancellationToken cancellationToken)
    {
        IReadOnlyList<TagEntity> tags = await lookupRepository.GetAllTagsAsync(cancellationToken: cancellationToken);

        var dtos = tags.Adapt<IReadOnlyList<TagDto>>();

        return new GetAllTagsResult(Tags: dtos);
    }
}
