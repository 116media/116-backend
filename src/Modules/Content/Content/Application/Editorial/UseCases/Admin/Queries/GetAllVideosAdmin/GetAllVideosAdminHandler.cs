using _116.Content.Application.Shared.DTOs;
using _116.Content.Application.Shared.Mappers;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Shared.Application.Pagination;
using _116.Shared.Contracts.Application.CQRS;
using MapsterMapper;

namespace _116.Content.Application.Editorial.UseCases.Admin.Queries.GetAllVideosAdmin;

/// <summary>
/// Handles the <see cref="GetAllVideosAdminQuery" /> to retrieve a paginated list of videos.
/// </summary>
/// <param name="videoRepository">Repository for video data access operations.</param>
/// <param name="mapper">Mapster mapper for entity-to-DTO transformations.</param>
public class GetAllVideosAdminHandler(IVideoRepository videoRepository, IMapper mapper)
    : IQueryHandler<GetAllVideosAdminQuery, GetAllVideosAdminResult>
{
    /// <inheritdoc />
    public async Task<GetAllVideosAdminResult> Handle(GetAllVideosAdminQuery query, CancellationToken cancellationToken)
    {
        int pageSize = query.PaginatedRequest.PageSize;
        int pageIndex = query.PaginatedRequest.PageIndex;

        (List<VideoEntity> videos, int totalCount) = await videoRepository.GetAllAsync(
            page: pageIndex + 1,
            pageSize: pageSize,
            status: query.Status,
            categoryId: query.CategoryId,
            cancellationToken: cancellationToken
        );

        List<VideoSummaryDto> dtoList = videos.Select(v => v.ToVideoSummaryDto(mapper)).ToList();

        var paginatedResult = new PaginatedResult<VideoSummaryDto>(
            pageIndex: pageIndex,
            pageSize: pageSize,
            count: totalCount,
            items: dtoList
        );

        return new GetAllVideosAdminResult(Videos: paginatedResult);
    }
}
