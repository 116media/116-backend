using _116.BuildingBlocks.Application.CQRS;
using _116.BuildingBlocks.Application.Pagination;
using _116.Content.Application.Editorial.Services;
using _116.Content.Application.Shared.DTOs;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;

namespace _116.Content.Application.Editorial.UseCases.Admin.Queries.GetAllShorts;

/// <summary>
/// Handles the <see cref="AdminGetAllShortsQuery" /> to page every short for the admin listing.
/// </summary>
/// <param name="shortVideoRepository">Repository paging the shorts.</param>
/// <param name="shortVideoDtoService">Service assembling the short DTOs with their authors.</param>
public class AdminGetAllShortsHandler(
    IShortVideoRepository shortVideoRepository,
    IShortVideoDtoService shortVideoDtoService
) : IQueryHandler<AdminGetAllShortsQuery, AdminGetAllShortsResult>
{
    /// <inheritdoc />
    public async Task<AdminGetAllShortsResult> Handle(AdminGetAllShortsQuery query, CancellationToken cancellationToken)
    {
        int pageSize = query.PaginatedRequest.PageSize;
        int pageIndex = query.PaginatedRequest.PageIndex;

        (List<ShortVideoEntity> shortVideos, int totalCount) = await shortVideoRepository.GetAllAsync(
            page: pageIndex + 1,
            pageSize: pageSize,
            search: query.Search,
            isActive: query.IsActive,
            cancellationToken: cancellationToken
        );

        IReadOnlyList<ShortVideoDto> dtoList = await shortVideoDtoService.CreateManyWithAuthorAsync(
            shortVideos,
            cancellationToken
        );

        var paginatedResult = new PaginatedResult<ShortVideoDto>(
            pageIndex: pageIndex,
            pageSize: pageSize,
            count: totalCount,
            items: dtoList
        );
        return new AdminGetAllShortsResult(ShortVideos: paginatedResult);
    }
}
