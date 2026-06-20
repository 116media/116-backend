using _116.Content.Application.Catalog.Constants;
using _116.Content.Application.Editorial.Constants;
using _116.Content.Application.Shared.Errors.Facade;
using _116.Content.Application.Shared.Mappers;
using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Core.Application.Shared.Repositories;
using _116.Shared.Contracts.Application.CQRS;
using MapsterMapper;

namespace _116.Content.Application.Catalog.UseCases.Admin.Commands.PinCategoryToFeed;

/// <summary>
/// Handles the <see cref="AdminPinCategoryToFeedCommand" /> to pin a category to the content feed.
/// Enforces the eligibility gate (active, video content type, minimum published videos) and the
/// per-content-type cap, unpinning the oldest pinned category (FIFO) when the cap is exceeded.
/// </summary>
/// <param name="categoryRepository">Repository for category data access operations.</param>
/// <param name="videoRepository">Repository for video data access operations.</param>
/// <param name="unitOfWork">Unit of Work for managing database transactions.</param>
/// <param name="fileRepository">Repository for resolving file URLs.</param>
/// <param name="mapper">Mapster mapper for entity-to-DTO transformations.</param>
/// <param name="i18n">Single i18n entry point for the Content module.</param>
public class AdminPinCategoryToFeedHandler(
    ICategoryRepository categoryRepository,
    IVideoRepository videoRepository,
    IContentUnitOfWork unitOfWork,
    IFileRepository fileRepository,
    IMapper mapper,
    ContentI18n i18n
) : ICommandHandler<AdminPinCategoryToFeedCommand, AdminPinCategoryToFeedResult>
{
    /// <inheritdoc />
    public async Task<AdminPinCategoryToFeedResult> Handle(
        AdminPinCategoryToFeedCommand command,
        CancellationToken cancellationToken
    )
    {
        Guid id = Guid.Parse(command.Id);

        CategoryEntity category = await categoryRepository.GetByIdOrThrowAsync(
            id: id,
            cancellationToken: cancellationToken
        );

        if (!category.IsActive)
        {
            throw i18n.Category.CannotPinInactiveToFeed();
        }

        // Only the video feed exists today, so only Video categories can be pinned.
        // Article categories become eligible when the article feed lands.
        if (category.ContentType.Name != nameof(EnumCoreContentType.Video))
        {
            throw i18n.Category.ContentTypeNotFeedable();
        }

        // Eligibility gate: a category needs enough published videos to fill a credible section.
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

        // FIFO eviction: only when pinning a NEW category that would exceed the cap.
        if (!alreadyPinned && pinned.Count >= CatalogFeedConstants.MaxPinnedCategoriesPerContentType)
        {
            CategoryEntity oldest = pinned.OrderBy(c => c.PinnedToFeedAt).First();
            oldest.UnpinFromFeed();
        }

        // Re-pinning an already-pinned category refreshes its timestamp (front of queue).
        category.PinToFeed();

        await unitOfWork.CommitAsync(cancellationToken: cancellationToken);

        CategoryEntity updated = await categoryRepository.GetByIdOrThrowAsync(
            id: id,
            cancellationToken: cancellationToken
        );

        var dto = await updated.ToCategoryDtoAsync(mapper, fileRepository, cancellationToken);
        return new AdminPinCategoryToFeedResult(Category: dto);
    }
}
