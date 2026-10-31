using _116.Content.Application.Shared.DTOs;
using _116.Content.Domain.Enums;

namespace _116.Content.Application.Shared.Search;

/// <summary>
/// The single port the search use case reads through. Answers from the search projection,
/// so neither this contract nor its callers name a content type.
/// </summary>
public interface ISearchCatalog
{
    /// <summary>
    /// Returns one ranked page for a scope. <see cref="EnumSearchScope.All" /> ranks across
    /// every type together rather than within each.
    /// </summary>
    /// <param name="scope">The scope to narrow to.</param>
    /// <param name="query">The reader's query, already trimmed.</param>
    /// <param name="skip">Rows to skip.</param>
    /// <param name="take">Rows to return.</param>
    /// <param name="cancellationToken">Token to observe for cancellation requests.</param>
    /// <returns>The page of hits, highest score first.</returns>
    Task<IReadOnlyList<SearchHitDto>> SearchAsync(
        EnumSearchScope scope,
        string query,
        int skip,
        int take,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Counts the hits for every scope, including a zero for the scopes that matched none.
    /// </summary>
    /// <param name="query">The reader's query, already trimmed.</param>
    /// <param name="cancellationToken">Token to observe for cancellation requests.</param>
    /// <returns>The counts per scope.</returns>
    Task<SearchCountsDto> CountAsync(string query, CancellationToken cancellationToken = default);
}
