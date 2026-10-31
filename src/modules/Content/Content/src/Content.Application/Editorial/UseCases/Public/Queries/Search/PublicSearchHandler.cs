using _116.BuildingBlocks.Application.CQRS;
using _116.BuildingBlocks.Application.Pagination;
using _116.Content.Application.Editorial.Services;
using _116.Content.Application.Shared.DTOs;
using _116.Content.Application.Shared.Search;

namespace _116.Content.Application.Editorial.UseCases.Public.Queries.Search;

/// <summary>
/// Handles the <see cref="PublicSearchQuery" />: reads one ranked page for the requested
/// scope and resolves its display fields in batches. Names no content type, so a new
/// searchable type needs no change here.
/// </summary>
/// <param name="searchCatalog">Catalog fanning the query out over the registered providers.</param>
/// <param name="searchHitDtoService">Service resolving posters and category names for a page of hits.</param>
public class PublicSearchHandler(ISearchCatalog searchCatalog, ISearchHitDtoService searchHitDtoService)
    : IQueryHandler<PublicSearchQuery, PublicSearchResult>
{
    /// <inheritdoc />
    public async Task<PublicSearchResult> Handle(PublicSearchQuery query, CancellationToken cancellationToken)
    {
        string term = query.Query.Trim();
        PaginatedRequest page = query.PaginatedRequest;

        SearchCountsDto counts = await searchCatalog.CountAsync(term, cancellationToken);

        IReadOnlyList<SearchHitDto> hits = await searchCatalog.SearchAsync(
            scope: query.Scope,
            query: term,
            skip: page.Skip,
            take: page.PageSize,
            cancellationToken: cancellationToken
        );

        IReadOnlyList<SearchHitDto> resolved = await searchHitDtoService.ResolveDisplayFieldsAsync(
            hits: hits,
            cancellationToken: cancellationToken
        );

        return new PublicSearchResult(
            Query: term,
            Scope: query.Scope,
            Counts: counts,
            Results: new PaginatedResult<SearchHitDto>(
                pageIndex: page.PageIndex,
                pageSize: page.PageSize,
                count: counts.For(query.Scope),
                items: resolved
            )
        );
    }
}
