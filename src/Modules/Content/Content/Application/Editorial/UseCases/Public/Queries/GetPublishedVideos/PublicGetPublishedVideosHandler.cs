using _116.Content.Application.Shared.DTOs;
using _116.Content.Application.Shared.Mappers;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Core.Contracts.Application.Services;
using _116.Shared.Application.Pagination;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Editorial.UseCases.Public.Queries.GetPublishedVideos;

/// <summary>
/// Handles the <see cref="PublicGetPublishedVideosQuery" /> to retrieve a paginated list of published videos.
/// </summary>
/// <param name="videoRepository">Repository for video data access operations.</param>
/// <param name="fileStorage">Core's storage contract.</param>
public class PublicGetPublishedVideosHandler(IVideoRepository videoRepository, IFileStorageService fileStorage)
    : IQueryHandler<PublicGetPublishedVideosQuery, PublicGetPublishedVideosResult>
{
    /// <inheritdoc />
    public async Task<PublicGetPublishedVideosResult> Handle(
        PublicGetPublishedVideosQuery query,
        CancellationToken cancellationToken
    )
    {
        int pageSize = query.PaginatedRequest.PageSize;
        int pageIndex = query.PaginatedRequest.PageIndex;

        (List<VideoEntity> videos, int totalCount) = await videoRepository.GetAllAsync(
            page: pageIndex + 1,
            pageSize: pageSize,
            search: query.Search,
            status: EnumContentStatus.Published,
            categoryId: query.CategoryId,
            tagSlug: query.TagSlug,
            cancellationToken: cancellationToken
        );

        IReadOnlyList<PublicVideoSummaryDto> dtoList = await videos.ToPublicVideoSummaryDtosAsync(
            fileStorage,
            cancellationToken
        );

        var paginatedResult = new PaginatedResult<PublicVideoSummaryDto>(
            pageIndex: pageIndex,
            pageSize: pageSize,
            count: totalCount,
            items: dtoList
        );

        return new PublicGetPublishedVideosResult(Videos: paginatedResult);
    }
}
