using _116.BuildingBlocks.Application.CQRS;
using _116.BuildingBlocks.Application.Pagination;
using _116.Content.Application.Editorial.Services;
using _116.Content.Application.Shared.DTOs;
using _116.Content.Application.Shared.Mappers;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Storage.Contracts.Application.Services;

namespace _116.Content.Application.Editorial.UseCases.Admin.Queries.GetAllArtists;

/// <summary>
/// Handles the <see cref="AdminGetAllArtistsQuery" /> to retrieve a paginated list of artist profiles.
/// </summary>
/// <param name="artistRepository">Repository for artist profile data access operations.</param>
/// <param name="artistDtoService">Builds artist projections with their avatars resolved.</param>
public class AdminGetAllArtistsHandler(IArtistRepository artistRepository, IArtistDtoService artistDtoService)
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

        IReadOnlyList<ArtistDto> dtoList = await artistDtoService.CreateManyAsync(
            artistList.AsReadOnly(),
            cancellationToken
        );

        var paginatedResult = new PaginatedResult<ArtistDto>(
            pageIndex: pageIndex,
            pageSize: pageSize,
            count: totalCount,
            items: dtoList
        );

        return new AdminGetAllArtistsResult(Artists: paginatedResult);
    }
}
