using _116.BuildingBlocks.Application.Pagination;
using _116.Content.Application.Catalog.UseCases.Public.Queries.GetExclusiveCategory.Contracts;
using _116.Content.Application.Editorial.Services;
using _116.Content.Application.Shared.DTOs;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;

namespace _116.Content.Application.Catalog.UseCases.Public.Queries.GetExclusiveCategory;

/// <summary>
/// Pages the exclusive category's published videos for the public exclusive-category query.
/// </summary>
/// <param name="videoRepository">Repository paging the published videos.</param>
/// <param name="videoDtoService">Service assembling the public video summaries.</param>
public class PublicExclusiveCategoryVideosService(IVideoRepository videoRepository, IVideoDtoService videoDtoService)
    : IPublicExclusiveCategoryVideosService
{
    /// <inheritdoc />
    public async Task<PaginatedResult<PublicVideoSummaryDto>> GetPublishedPageAsync(
        Guid categoryId,
        int pageIndex,
        int pageSize,
        CancellationToken cancellationToken
    )
    {
        (List<VideoEntity> videos, int totalCount) = await videoRepository.GetAllAsync(
            search: null,
            pageSize: pageSize,
            page: pageIndex + 1,
            categoryId: categoryId,
            status: EnumContentStatus.Published,
            cancellationToken: cancellationToken
        );

        IReadOnlyList<PublicVideoSummaryDto> videoDtos = await videoDtoService.CreatePublicManyAsync(
            videos,
            cancellationToken
        );

        return new PaginatedResult<PublicVideoSummaryDto>(
            items: videoDtos,
            count: totalCount,
            pageSize: pageSize,
            pageIndex: pageIndex
        );
    }
}
