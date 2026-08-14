using _116.Content.Application.Shared.DTOs;
using _116.Content.Application.Shared.Repositories;
using _116.Core.Application.Shared.Repositories;
using _116.Core.Domain.Entities;
using _116.Shared.Application.Pagination;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Editorial.UseCases.Public.Queries.GetArtists;

/// <summary>
/// Handles the <see cref="PublicGetArtistsQuery" /> to serve the public artist directory.
/// The repository returns the filter, ordering and per-row content count from one
/// statement; this handler only resolves avatar URLs and shapes the page.
/// </summary>
/// <param name="artistRepository">Repository for artist profile data access operations.</param>
/// <param name="fileRepository">Repository for resolving avatar file URLs.</param>
public class PublicGetArtistsHandler(IArtistRepository artistRepository, IFileRepository fileRepository)
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

        var cards = new List<ArtistSummaryDto>(capacity: rows.Count);

        foreach (ArtistDirectoryRow row in rows)
        {
            string? avatarUrl = null;

            if (row.Artist.AvatarFileId.HasValue)
            {
                FileEntity? avatarFile = await fileRepository.GetByIdAsync(
                    row.Artist.AvatarFileId.Value,
                    cancellationToken
                );
                avatarUrl = avatarFile?.StorageUrl;
            }

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
