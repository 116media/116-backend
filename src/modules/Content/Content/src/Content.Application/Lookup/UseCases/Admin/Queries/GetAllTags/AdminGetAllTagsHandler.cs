using _116.BuildingBlocks.Application.CQRS;
using _116.Content.Application.Shared.DTOs;
using _116.Content.Application.Shared.Mappers;
using _116.Content.Application.Shared.Repositories;

namespace _116.Content.Application.Lookup.UseCases.Admin.Queries.GetAllTags;

/// <summary>
/// Handles the <see cref="AdminGetAllTagsQuery" /> to retrieve all tags.
/// </summary>
/// <param name="tagRepository">Repository for tag data access operations.</param>
public class AdminGetAllTagsHandler(ITagRepository tagRepository)
    : IQueryHandler<AdminGetAllTagsQuery, AdminGetAllTagsResult>
{
    /// <inheritdoc />
    public async Task<AdminGetAllTagsResult> Handle(AdminGetAllTagsQuery query, CancellationToken cancellationToken)
    {
        IReadOnlyList<TagCountRow> rows = await tagRepository.GetAllAsync(
            search: query.Search,
            publishedOnly: false,
            cancellationToken: cancellationToken
        );

        return new AdminGetAllTagsResult(Tags: rows.ToTagWithCountDtos());
    }
}
