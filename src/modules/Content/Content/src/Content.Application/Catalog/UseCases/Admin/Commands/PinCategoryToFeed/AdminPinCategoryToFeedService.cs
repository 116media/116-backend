using _116.Content.Application.Catalog.Constants;
using _116.Content.Application.Catalog.UseCases.Admin.Commands.PinCategoryToFeed.Contracts;
using _116.Content.Application.Editorial.Constants;
using _116.Content.Application.Shared.Errors.Facade;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;

namespace _116.Content.Application.Catalog.UseCases.Admin.Commands.PinCategoryToFeed;

/// <summary>
/// Resolves and applies a feed pin for the admin pin-to-feed use case.
/// </summary>
/// <param name="categoryRepository">Repository loading the category and the current pins.</param>
/// <param name="contentTypeRepository">Repository resolving the category's content type.</param>
/// <param name="videoRepository">Repository counting the category's published videos.</param>
/// <param name="i18n">Single i18n entry point for the Content module.</param>
/// <param name="timeProvider">Clock stamping the pin.</param>
public class AdminPinCategoryToFeedService(
    ICategoryRepository categoryRepository,
    IContentTypeRepository contentTypeRepository,
    IVideoRepository videoRepository,
    ContentI18n i18n,
    TimeProvider timeProvider
) : IAdminPinCategoryToFeedService
{
    /// <inheritdoc />
    public async Task<CategoryEntity> PinAsync(Guid categoryId, CancellationToken cancellationToken)
    {
        CategoryEntity category = await categoryRepository.GetByIdOrThrowAsync(
            id: categoryId,
            cancellationToken: cancellationToken
        );
        ContentTypeEntity contentType = await contentTypeRepository.GetByIdOrThrowAsync(
            id: category.ContentTypeId,
            cancellationToken: cancellationToken
        );

        if (!category.IsActive)
        {
            throw i18n.Category.CannotPinInactiveToFeed();
        }

        // Only the video feed exists today, so only Video categories can be pinned.
        if (contentType.Name != nameof(EnumCoreContentType.Video))
        {
            throw i18n.Category.ContentTypeNotFeedable();
        }

        int publishedCount = await videoRepository.CountPublishedByCategoryAsync(
            categoryId: category.Id,
            cancellationToken: cancellationToken
        );

        if (publishedCount < EditorialFeedConstants.MinVideosToPinToFeed)
        {
            throw i18n.Category.NotEnoughVideosToPinToFeed(EditorialFeedConstants.MinVideosToPinToFeed);
        }

        IReadOnlyList<CategoryEntity> pinned = await categoryRepository.GetPinnedToFeedCategoriesAsync(
            contentTypeId: category.ContentTypeId,
            cancellationToken: cancellationToken
        );

        bool alreadyPinned = pinned.Any(c => c.Id == category.Id);

        // FIFO eviction only when a new pin would exceed the cap.
        if (!alreadyPinned && pinned.Count >= CatalogFeedConstants.MaxPinnedCategoriesPerContentType)
        {
            CategoryEntity oldest = pinned.OrderBy(c => c.PinnedToFeedAt).First();
            oldest.UnpinFromFeed();
        }

        // Re-pinning an already-pinned category refreshes its timestamp.
        category.PinToFeed(now: timeProvider.GetUtcNow());

        return category;
    }
}
