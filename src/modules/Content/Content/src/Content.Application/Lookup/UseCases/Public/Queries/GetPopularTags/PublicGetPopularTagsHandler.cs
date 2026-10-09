using _116.BuildingBlocks.Application.CQRS;
using _116.Content.Application.Shared.DTOs;
using _116.Content.Application.Shared.Mappers;
using _116.Content.Application.Shared.Repositories;

namespace _116.Content.Application.Lookup.UseCases.Public.Queries.GetPopularTags;

/// <summary>
/// Handles the <see cref="PublicGetPopularTagsQuery" /> to retrieve the most-used tags.
/// </summary>
/// <param name="tagRepository">Repository for tag data access operations.</param>
public class PublicGetPopularTagsHandler(ITagRepository tagRepository)
    : IQueryHandler<PublicGetPopularTagsQuery, PublicGetPopularTagsResult>
{
    /// <inheritdoc />
    public async Task<PublicGetPopularTagsResult> Handle(
        PublicGetPopularTagsQuery query,
        CancellationToken cancellationToken
    )
    {
        IReadOnlyList<TagCountRow> rows = await tagRepository.GetPopularAsync(
            limit: query.Limit,
            contentType: query.ContentType,
            publishedOnly: true,
            cancellationToken: cancellationToken
        );

        return new PublicGetPopularTagsResult(Tags: rows.ToTagWithCountDtos());
    }
}
