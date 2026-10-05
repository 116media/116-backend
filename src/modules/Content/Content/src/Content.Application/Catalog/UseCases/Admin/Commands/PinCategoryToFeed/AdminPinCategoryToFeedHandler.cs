using _116.BuildingBlocks.Application.CQRS;
using _116.Content.Application.Catalog.Services;
using _116.Content.Application.Catalog.UseCases.Admin.Commands.PinCategoryToFeed.Contracts;
using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;

namespace _116.Content.Application.Catalog.UseCases.Admin.Commands.PinCategoryToFeed;

/// <summary>
/// Handles the <see cref="AdminPinCategoryToFeedCommand" /> to pin a category to the video feed.
/// </summary>
/// <param name="pinService">Service gating and applying the pin.</param>
/// <param name="categoryRepository">Repository reloading the committed category.</param>
/// <param name="unitOfWork">Unit of Work for managing database transactions.</param>
/// <param name="categoryDtoService">Service assembling the category DTO.</param>
public class AdminPinCategoryToFeedHandler(
    IAdminPinCategoryToFeedService pinService,
    ICategoryRepository categoryRepository,
    IContentUnitOfWork unitOfWork,
    ICategoryDtoService categoryDtoService
) : ICommandHandler<AdminPinCategoryToFeedCommand, AdminPinCategoryToFeedResult>
{
    /// <inheritdoc />
    public async Task<AdminPinCategoryToFeedResult> Handle(
        AdminPinCategoryToFeedCommand command,
        CancellationToken cancellationToken
    )
    {
        Guid id = Guid.Parse(command.Id);

        await pinService.PinAsync(categoryId: id, cancellationToken: cancellationToken);
        await unitOfWork.CommitAsync(cancellationToken: cancellationToken);

        CategoryEntity updated = await categoryRepository.GetByIdOrThrowAsync(
            id: id,
            cancellationToken: cancellationToken
        );
        var dto = await categoryDtoService.CreateAsync(updated, cancellationToken);
        return new AdminPinCategoryToFeedResult(Category: dto);
    }
}
