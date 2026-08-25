using _116.Content.Application.Shared.DTOs;
using _116.Content.Application.Shared.Mappers;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Shared.Contracts.Application.CQRS;
using MapsterMapper;

namespace _116.Content.Application.Lookup.UseCases.Public.Queries.GetPopularTags;

/// <summary>
/// Handles the <see cref="PublicGetPopularTagsQuery" /> to retrieve the most-used tags.
/// </summary>
/// <param name="tagRepository">Repository for tag data access operations.</param>
/// <param name="mapper">Mapster mapper for entity-to-DTO transformations.</param>
public class PublicGetPopularTagsHandler(ITagRepository tagRepository, IMapper mapper)
    : IQueryHandler<PublicGetPopularTagsQuery, PublicGetPopularTagsResult>
{
    /// <inheritdoc />
    public async Task<PublicGetPopularTagsResult> Handle(
        PublicGetPopularTagsQuery query,
        CancellationToken cancellationToken
    )
    {
        IReadOnlyList<TagEntity> tags = await tagRepository.GetPopularAsync(
            limit: query.Limit,
            contentType: query.ContentType,
            cancellationToken: cancellationToken
        );

        return new PublicGetPopularTagsResult(Tags: tags.ToTagDtos(mapper));
    }
}
