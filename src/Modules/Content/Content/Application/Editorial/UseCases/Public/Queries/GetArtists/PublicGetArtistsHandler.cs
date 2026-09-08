using _116.Content.Application.Shared.DTOs;
using _116.Content.Application.Shared.Repositories;
using _116.Core.Contracts.Application.Services;
using _116.Shared.Application.Pagination;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Editorial.UseCases.Public.Queries.GetArtists;

/// <summary>
/// Handles the <see cref="PublicGetArtistsQuery" /> to serve the public artist directory.
/// The repository returns the filter, ordering and per-row content count from one
/// statement; this handler only resolves avatar URLs and shapes the page.
/// </summary>
/// <param name="artistRepository">Repository for artist profile data access operations.</param>
/// <param name="fileStorage">Core's storage contract.</param>
public class PublicGetArtistsHandler(IArtistRepository artistRepository, IFileStorageService fileStorage)
    : IQueryHandler<PublicGetArtistsQuery, PublicGetArtistsResult>
{
    /// <inheritdoc />
    public async Task<PublicGetArtistsResult> Handle(PublicGetArtistsQuery query, CancellationToken cancellationToken)
    {
        (List<ArtistDirectoryRow> rows, int totalCount) = await artistRepository.GetPublicDirectoryAsync(
            page: query.Page.PageIndex + 1,
            pageSize: query.Page.PageSize,
            letter: query.Letter,
            search: query.Search,
            cancellationToken: cancellationToken
        );

        IReadOnlyList<string> availableLetters = await artistRepository.GetAvailableLettersAsync(
            cancellationToken: cancellationToken
        );

        IReadOnlyDictionary<Guid, string> avatarUrls = await fileStorage.ResolveUrlsAsync(
            rows.Where(r => r.Artist.AvatarFileId.HasValue).Select(r => r.Artist.AvatarFileId!.Value).ToHashSet(),
            cancellationToken
        );

        var cards = new List<ArtistSummaryDto>(capacity: rows.Count);

        foreach (ArtistDirectoryRow row in rows)
        {
            string? avatarUrl = row.Artist.AvatarFileId.HasValue
                ? avatarUrls.GetValueOrDefault(row.Artist.AvatarFileId.Value)
                : null;

            cards.Add(
                new ArtistSummaryDto(
                    Name: row.Artist.Name,
                    Slug: row.Artist.Slug,
                    AvatarUrl: avatarUrl,
                    IsVerified: row.Artist.UserId is not null && row.Artist.VerifiedAt is not null,
                    ContentCount: row.ContentCount
                )
            );
        }

        var artists = new PaginatedResult<ArtistSummaryDto>(
            pageIndex: query.Page.PageIndex,
            pageSize: query.Page.PageSize,
            count: totalCount,
            items: cards
        );

        return new PublicGetArtistsResult(Artists: artists, AvailableLetters: availableLetters);
    }
}
