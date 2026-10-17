using _116.Content.Application.Shared.DTOs;

namespace _116.Content.Application.Editorial.Services;

/// <summary>
/// Fills the display fields a search hit cannot carry out of the database: the poster
/// URL from Storage and the category name from the catalogue, each in one batch.
/// </summary>
public interface ISearchHitDtoService
{
    /// <summary>
    /// Returns the hits with their poster URLs, category names and excerpts resolved.
    /// </summary>
    /// <param name="hits">The hits to complete.</param>
    /// <param name="cancellationToken">Token to observe for cancellation requests.</param>
    /// <returns>The completed hits, in the order given.</returns>
    Task<IReadOnlyList<SearchHitDto>> ResolveDisplayFieldsAsync(
        IReadOnlyList<SearchHitDto> hits,
        CancellationToken cancellationToken = default
    );
}
