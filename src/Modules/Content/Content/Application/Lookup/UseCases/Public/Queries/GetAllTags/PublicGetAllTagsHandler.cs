using _116.Content.Application.Shared.DTOs;
using _116.Content.Application.Shared.Mappers;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Shared.Contracts.Application.CQRS;
using MapsterMapper;

namespace _116.Content.Application.Lookup.UseCases.Public.Queries.GetAllTags;

/// <summary>
/// Handles the <see cref="PublicGetAllTagsQuery" /> to retrieve all tags.
/// </summary>
/// <param name="tagRepository">Repository for tag data access operations.</param>
/// <param name="mapper">Mapster mapper for entity-to-DTO transformations.</param>
public class PublicGetAllTagsHandler(ITagRepository tagRepository, IMapper mapper)
    : IQueryHandler<PublicGetAllTagsQuery, PublicGetAllTagsResult>
{
    /// <inheritdoc />
    public async Task<PublicGetAllTagsResult> Handle(PublicGetAllTagsQuery query, CancellationToken cancellationToken)
    {
        IReadOnlyList<TagEntity> tags = await tagRepository.GetAllAsync(
            search: query.Search,
            contentType: query.ContentType,
            limit: query.Limit,
            cancellationToken: cancellationToken
        );

        return new PublicGetAllTagsResult(Tags: tags.ToTagDtos(mapper));
    }
}
