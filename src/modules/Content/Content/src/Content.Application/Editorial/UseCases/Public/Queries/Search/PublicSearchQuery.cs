using _116.BuildingBlocks.Application.CQRS;
using _116.BuildingBlocks.Application.Pagination;
using _116.Content.Application.Shared.DTOs;
using _116.Content.Domain.Enums;

namespace _116.Content.Application.Editorial.UseCases.Public.Queries.Search;

/// <summary>
/// Query for the public cross-content search. Returns the ranked hits for the active
/// scope together with the hit counts for every scope.
/// </summary>
/// <param name="PaginatedRequest">Pagination parameters.</param>
/// <param name="Query">The reader's query; trimmed and validated to 2-120 characters.</param>
/// <param name="Scope">The content types to narrow to.</param>
public record PublicSearchQuery(
    PaginatedRequest PaginatedRequest,
    string Query,
    EnumSearchScope Scope = EnumSearchScope.All
) : IQuery<PublicSearchResult>;

/// <summary>
/// Result of the <see cref="PublicSearchQuery" />: the ranked hits for the active scope
/// plus the per-scope counts for the whole query.
/// </summary>
/// <param name="Query">The trimmed query that was run, echoed for client display.</param>
/// <param name="Scope">The scope the results were narrowed to.</param>
/// <param name="Counts">Hit counts per scope.</param>
/// <param name="Results">The paginated hits for the active scope.</param>
public record PublicSearchResult(
    string Query,
    EnumSearchScope Scope,
    SearchCountsDto Counts,
    PaginatedResult<SearchHitDto> Results
);
