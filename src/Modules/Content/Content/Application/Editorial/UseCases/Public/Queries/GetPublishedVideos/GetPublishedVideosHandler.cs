using _116.Content.Application.Shared.DTOs;
using _116.Content.Application.Shared.Mappers;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Shared.Application.Pagination;
using _116.Shared.Contracts.Application.CQRS;
using MapsterMapper;

namespace _116.Content.Application.Editorial.UseCases.Public.Queries.GetPublishedVideos;

/// <summary>
/// Handles the <see cref="GetPublishedVideosQuery" /> to retrieve a paginated list of published videos.
/// </summary>
/// <param name="videoRepository">Repository for video data access operations.</param>
/// <param name="mapper">Mapster mapper for entity-to-DTO transformations.</param>
public class GetPublishedVideosHandler(IVideoRepository videoRepository, IMapper mapper)
    : IQueryHandler<GetPublishedVideosQuery, GetPublishedVideosResult>
{
    /// <inheritdoc />
    public async Task<GetPublishedVideosResult> Handle(
        GetPublishedVideosQuery query,
        CancellationToken cancellationToken
    )
    {
        int pageSize = query.PaginatedRequest.PageSize;
        int pageIndex = query.PaginatedRequest.PageIndex;

        (List<VideoEntity> videos, int totalCount) = await videoRepository.GetAllAsync(
            page: pageIndex + 1,
            pageSize: pageSize,
            status: EnumContentStatus.Published,
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

        return new GetPublishedVideosResult(Videos: paginatedResult);
    }
}
