using _116.Content.Application.Shared.DTOs;
using _116.Content.Application.Shared.Helpers;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Storage.Contracts.Application.Services;

namespace _116.Content.Application.Editorial.Services;

/// <summary>
/// Resolves the poster URLs and category names for a page of search hits, one batch per
/// source, so completing a page costs two queries rather than two per row.
/// </summary>
/// <param name="fileStorageService">Service resolving storage URLs for poster files.</param>
/// <param name="categoryRepository">Repository resolving categories by identifier.</param>
public class SearchHitDtoService(IFileStorageService fileStorageService, ICategoryRepository categoryRepository)
    : ISearchHitDtoService
{
    /// <inheritdoc />
    public async Task<IReadOnlyList<SearchHitDto>> ResolveDisplayFieldsAsync(
        IReadOnlyList<SearchHitDto> hits,
        CancellationToken cancellationToken = default
    )
    {
        if (hits.Count == 0)
        {
            return hits;
        }

        Guid[] fileIds =
        [
            .. hits.Where(hit => hit.ImageFileId.HasValue).Select(hit => hit.ImageFileId!.Value).Distinct(),
        ];
        Guid[] categoryIds =
        [
            .. hits.Where(hit => hit.CategoryId.HasValue).Select(hit => hit.CategoryId!.Value).Distinct(),
        ];

        IReadOnlyDictionary<Guid, string> urls =
            fileIds.Length == 0
                ? new Dictionary<Guid, string>()
                : await fileStorageService.ResolveUrlsAsync(fileIds, cancellationToken);

        IReadOnlyDictionary<Guid, CategoryEntity> categories =
            categoryIds.Length == 0
                ? new Dictionary<Guid, CategoryEntity>()
                : await categoryRepository.GetByIdsAsync(categoryIds, cancellationToken);

        return
        [
            .. hits.Select(hit =>
                hit with
                {
                    ImageUrl =
                        hit.ImageFileId.HasValue && urls.TryGetValue(hit.ImageFileId.Value, out string? url)
                            ? url
                            : null,
                    CategoryName =
                        hit.CategoryId.HasValue
                        && categories.TryGetValue(hit.CategoryId.Value, out CategoryEntity? category)
                            ? category.Name
                            : null,
                    Excerpt = ExcerptHelper.Truncate(hit.Excerpt),
                }
            ),
        ];
    }
}
