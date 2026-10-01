using _116.BuildingBlocks.Application.CQRS;
using _116.BuildingBlocks.Application.Pagination;
using _116.Content.Application.Catalog.Services;
using _116.Content.Application.Catalog.UseCases.Public.Queries.GetExclusiveCategory.Contracts;
using _116.Content.Application.Shared.DTOs;
using _116.Content.Application.Shared.Errors.Facade;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;

namespace _116.Content.Application.Catalog.UseCases.Public.Queries.GetExclusiveCategory;

/// <summary>
/// Handles the <see cref="PublicGetExclusiveCategoryQuery" /> to serve the exclusive category with
/// a page of its published videos.
/// </summary>
/// <param name="categoryRepository">Repository resolving the exclusive category.</param>
/// <param name="categoryDtoService">Service assembling the category DTO.</param>
/// <param name="videosService">Service paging the category's published videos.</param>
/// <param name="i18n">Single i18n entry point for the Content module.</param>
public class PublicGetExclusiveCategoryHandler(
    ICategoryRepository categoryRepository,
    ICategoryDtoService categoryDtoService,
    IPublicExclusiveCategoryVideosService videosService,
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

        CategoryDto categoryDto = await categoryDtoService.CreateAsync(category, cancellationToken);
        PaginatedResult<PublicVideoSummaryDto> videos = await videosService.GetPublishedPageAsync(
            categoryId: category.Id,
            pageIndex: query.PaginatedRequest.PageIndex,
            pageSize: query.PaginatedRequest.PageSize,
            cancellationToken: cancellationToken
        );

        return new PublicGetExclusiveCategoryResult(Category: categoryDto, Videos: videos);
    }
}
