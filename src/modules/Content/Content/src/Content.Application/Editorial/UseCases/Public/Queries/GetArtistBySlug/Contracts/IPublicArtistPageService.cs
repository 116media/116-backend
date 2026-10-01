using _116.BuildingBlocks.Application.Pagination;
using _116.Content.Application.Shared.DTOs;

namespace _116.Content.Application.Editorial.UseCases.Public.Queries.GetArtistBySlug.Contracts;

/// <summary>
/// Pages an artist's published lyrics and videos into public summaries.
/// </summary>
public interface IPublicArtistPageService
{
    /// <summary>
    /// Loads one page of the artist's published lyrics and assembles their public summaries.
    /// </summary>
    /// <param name="artistId">The artist.</param>
    /// <param name="page">The requested page.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    Task<PaginatedResult<PublicLyricsSummaryDto>> GetLyricsPageAsync(
        Guid artistId,
        PaginatedRequest page,
        CancellationToken cancellationToken
    );

    /// <summary>
    /// Loads one page of the artist's published videos and assembles their public summaries.
    /// </summary>
    /// <param name="artistId">The artist.</param>
    /// <param name="page">The requested page.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    Task<PaginatedResult<PublicVideoSummaryDto>> GetVideosPageAsync(
        Guid artistId,
        PaginatedRequest page,
        CancellationToken cancellationToken
    );
}
