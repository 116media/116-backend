using _116.Content.Application.Shared.DTOs;
using _116.Content.Application.Shared.Mappers;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Shared.Application.Pagination;
using _116.Shared.Contracts.Application.CQRS;
using MapsterMapper;

namespace _116.Content.Application.Editorial.UseCases.Admin.Queries.GetAllShorts;

/// <summary>
/// Handles the <see cref="AdminGetAllShortsQuery" /> to retrieve a paginated list of short videos.
/// </summary>
/// <param name="shortVideoRepository">Repository for short video data access operations.</param>
/// <param name="mapper">Mapster mapper for entity-to-DTO transformations.</param>
public class AdminGetAllShortsHandler(IShortVideoRepository shortVideoRepository, IMapper mapper)
    : IQueryHandler<AdminGetAllShortsQuery, AdminGetAllShortsResult>
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

        List<ShortVideoDto> dtoList = shortVideos.Select(sv => sv.ToShortVideoDto(mapper)).ToList();

        var paginatedResult = new PaginatedResult<ShortVideoDto>(
            pageIndex: pageIndex,
            pageSize: pageSize,
            count: totalCount,
            items: dtoList
        );

        return new AdminGetAllShortsResult(ShortVideos: paginatedResult);
    }
}
