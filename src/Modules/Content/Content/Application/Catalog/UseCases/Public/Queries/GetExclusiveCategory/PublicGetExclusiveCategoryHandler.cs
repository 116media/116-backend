using _116.Content.Application.Catalog.Factories;
using _116.Content.Application.Editorial.Factories;
using _116.Content.Application.Shared.DTOs;
using _116.Content.Application.Shared.Errors.Facade;
using _116.Content.Application.Shared.Mappers;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Shared.Application.Pagination;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Catalog.UseCases.Public.Queries.GetExclusiveCategory;

/// <summary>
/// Handles the <see cref="PublicGetExclusiveCategoryQuery" /> to retrieve the exclusive category
/// along with a paginated list of its published videos.
/// </summary>
/// <param name="categoryRepository">Repository for category data access operations.</param>
/// <param name="videoRepository">Repository for video data access operations.</param>
/// <param name="categoryDtoFactory">Builds category projections with their posters resolved.</param>
/// <param name="fileStorage">Core's storage contract, for the video thumbnails listed alongside.</param>
/// <param name="i18n">Single i18n entry point for the Content module.</param>
/// <param name="videoDtoFactory">Builds video projections with their thumbnails resolved.</param>
public class PublicGetExclusiveCategoryHandler(
    ICategoryRepository categoryRepository,
    IVideoRepository videoRepository,
    ICategoryDtoFactory categoryDtoFactory,
    IVideoDtoFactory videoDtoFactory,
    ContentI18n i18n
) : IQueryHandler<PublicGetExclusiveCategoryQuery, PublicGetExclusiveCategoryResult>
{
    /// <inheritdoc />
    public async Task<PublicGetExclusiveCategoryResult> Handle(
        PublicGetExclusiveCategoryQuery query,
        CancellationToken cancellationToken
    )
    {
        CategoryEntity? category = await categoryRepository.GetExclusiveCategoryAsync(
            cancellationToken: cancellationToken
        );

        if (category is null)
        {
            throw i18n.Category.NoExclusiveCategoryFound();
        }

        CategoryDto categoryDto = await categoryDtoFactory.CreateAsync(category, cancellationToken);

        int pageSize = query.PaginatedRequest.PageSize;
        int pageIndex = query.PaginatedRequest.PageIndex;

        (List<VideoEntity> videos, int totalCount) = await videoRepository.GetAllAsync(
            search: null,
            pageSize: pageSize,
            page: pageIndex + 1,
            categoryId: category.Id,
            status: EnumContentStatus.Published,
            cancellationToken: cancellationToken
        );

        IReadOnlyList<PublicVideoSummaryDto> videoDtos = await videoDtoFactory.CreatePublicManyAsync(
            videos,
            cancellationToken
        );

        var paginatedVideos = new PaginatedResult<PublicVideoSummaryDto>(
            items: videoDtos,
            count: totalCount,
            pageSize: pageSize,
            pageIndex: pageIndex
        );

        return new PublicGetExclusiveCategoryResult(Category: categoryDto, Videos: paginatedVideos);
    }
}
