using _116.BuildingBlocks.Application.CQRS;
using _116.Content.Application.Shared.DTOs;
using _116.Content.Application.Shared.Mappers;
using _116.Content.Application.Shared.Repositories;

namespace _116.Content.Application.Lookup.UseCases.Public.Queries.GetAllTags;

/// <summary>
/// Handles the <see cref="PublicGetAllTagsQuery" /> to retrieve all tags.
/// </summary>
/// <param name="tagRepository">Repository for tag data access operations.</param>
public class PublicGetAllTagsHandler(ITagRepository tagRepository)
    : IQueryHandler<PublicGetAllTagsQuery, PublicGetAllTagsResult>
{
    /// <inheritdoc />
    public async Task<PublicGetAllTagsResult> Handle(PublicGetAllTagsQuery query, CancellationToken cancellationToken)
    {
        IReadOnlyList<TagCountRow> rows = await tagRepository.GetAllAsync(
            search: query.Search,
            contentType: query.ContentType,
            limit: query.Limit,
            publishedOnly: true,
            cancellationToken: cancellationToken
        );

        return new PublicGetAllTagsResult(Tags: rows.ToTagWithCountDtos());
    }
}
