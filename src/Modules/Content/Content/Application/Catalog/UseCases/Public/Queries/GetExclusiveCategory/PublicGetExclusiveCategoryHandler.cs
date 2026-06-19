using _116.Content.Application.Shared.DTOs;
using _116.Content.Application.Shared.Errors.Facade;
using _116.Content.Application.Shared.Mappers;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Core.Application.Shared.Repositories;
using _116.Shared.Application.Pagination;
using _116.Shared.Contracts.Application.CQRS;
using MapsterMapper;

namespace _116.Content.Application.Catalog.UseCases.Public.Queries.GetExclusiveCategory;

/// <summary>
/// Handles the <see cref="PublicGetExclusiveCategoryQuery" /> to retrieve the exclusive category
/// along with a paginated list of its published videos.
/// </summary>
/// <param name="categoryRepository">Repository for category data access operations.</param>
/// <param name="videoRepository">Repository for video data access operations.</param>
/// <param name="fileRepository">Repository for resolving file URLs.</param>
/// <param name="mapper">Mapster mapper for entity-to-DTO transformations.</param>
/// <param name="i18n">Single i18n entry point for the Content module.</param>
public class PublicGetExclusiveCategoryHandler(
    ICategoryRepository categoryRepository,
    IVideoRepository videoRepository,
    IFileRepository fileRepository,
    IMapper mapper,
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

        CategoryDto categoryDto = await category.ToCategoryDtoAsync(mapper, fileRepository, cancellationToken);

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

        IReadOnlyList<VideoSummaryDto> videoDtos = await videos.ToVideoSummaryDtosAsync(
            mapper,
            fileRepository,
            cancellationToken
        );

        var paginatedVideos = new PaginatedResult<VideoSummaryDto>(
            items: videoDtos,
            count: totalCount,
            pageSize: pageSize,
            pageIndex: pageIndex
        );

        return new PublicGetExclusiveCategoryResult(Category: categoryDto, Videos: paginatedVideos);
    }
}
