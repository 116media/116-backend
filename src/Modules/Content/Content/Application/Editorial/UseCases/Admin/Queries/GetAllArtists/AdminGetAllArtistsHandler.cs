using _116.Content.Application.Shared.DTOs;
using _116.Content.Application.Shared.Mappers;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Core.Contracts.Application.Services;
using _116.Shared.Application.Pagination;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Editorial.UseCases.Admin.Queries.GetAllArtists;

/// <summary>
/// Handles the <see cref="AdminGetAllArtistsQuery" /> to retrieve a paginated list of artist profiles.
/// </summary>
/// <param name="artistRepository">Repository for artist profile data access operations.</param>
/// <param name="fileStorage">Core's storage contract.</param>
public class AdminGetAllArtistsHandler(IArtistRepository artistRepository, IFileStorageService fileStorage)
    : IQueryHandler<AdminGetAllArtistsQuery, AdminGetAllArtistsResult>
{
    /// <inheritdoc />
    public async Task<AdminGetAllArtistsResult> Handle(
        AdminGetAllArtistsQuery query,
        CancellationToken cancellationToken
    )
    {
        int pageSize = query.PaginatedRequest.PageSize;
        int pageIndex = query.PaginatedRequest.PageIndex;

        (List<ArtistEntity> artistList, int totalCount) = await artistRepository.GetAllAsync(
            page: pageIndex + 1,
            pageSize: pageSize,
            search: query.Search,
            cancellationToken: cancellationToken
        );

        IReadOnlyList<ArtistDto> dtoList = await artistList
            .AsReadOnly()
            .ToArtistDtosAsync(fileStorage, cancellationToken);

        var paginatedResult = new PaginatedResult<ArtistDto>(
            pageIndex: pageIndex,
            pageSize: pageSize,
            count: totalCount,
            items: dtoList
        );

        return new AdminGetAllArtistsResult(Artists: paginatedResult);
    }
}
