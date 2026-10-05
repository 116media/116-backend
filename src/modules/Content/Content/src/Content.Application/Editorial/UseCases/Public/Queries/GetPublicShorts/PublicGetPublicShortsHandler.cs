using _116.BuildingBlocks.Application.CQRS;
using _116.BuildingBlocks.Application.Pagination;
using _116.Content.Application.Editorial.Services;
using _116.Content.Application.Shared.DTOs;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;

namespace _116.Content.Application.Editorial.UseCases.Public.Queries.GetPublicShorts;

/// <summary>
/// Handles the <see cref="PublicGetPublicShortsQuery" /> to page the active shorts.
/// </summary>
/// <param name="shortVideoRepository">Repository paging the shorts and resolving the viewer's interaction state.</param>
/// <param name="shortVideoDtoService">Service assembling the public short DTOs.</param>
public class PublicGetPublicShortsHandler(
    IShortVideoRepository shortVideoRepository,
    IShortVideoDtoService shortVideoDtoService
) : IQueryHandler<PublicGetPublicShortsQuery, PublicGetPublicShortsResult>
{
    /// <inheritdoc />
    public async Task<PublicGetPublicShortsResult> Handle(
        PublicGetPublicShortsQuery query,
        CancellationToken cancellationToken
    )
    {
        int pageSize = query.PaginatedRequest.PageSize;
        int pageIndex = query.PaginatedRequest.PageIndex;

        (List<ShortVideoEntity> shortVideos, int totalCount) = await shortVideoRepository.GetAllAsync(
            page: pageIndex + 1,
            pageSize: pageSize,
            search: query.Search,
            isActive: true,
            cancellationToken: cancellationToken
        );

        (IReadOnlySet<Guid> liked, IReadOnlySet<Guid> bookmarked) =
            await shortVideoRepository.GetLikedAndBookmarkedIdsAsync(
                currentUserId: query.CurrentUserId,
                shortVideoIds: shortVideos.Select(shortVideo => shortVideo.Id).ToList(),
                cancellationToken: cancellationToken
            );

        IReadOnlyList<PublicShortVideoDto> dtoList = await shortVideoDtoService.CreatePublicManyAsync(
            shortVideos,
            liked,
            bookmarked,
            cancellationToken
        );

        var paginatedResult = new PaginatedResult<PublicShortVideoDto>(
            pageIndex: pageIndex,
            pageSize: pageSize,
            count: totalCount,
            items: dtoList
        );
        return new PublicGetPublicShortsResult(ShortVideos: paginatedResult);
    }
}
