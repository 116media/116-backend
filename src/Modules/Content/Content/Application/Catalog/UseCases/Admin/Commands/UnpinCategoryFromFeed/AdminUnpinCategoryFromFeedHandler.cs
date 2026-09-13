using _116.Content.Application.Catalog.Factories;
using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Catalog.UseCases.Admin.Commands.UnpinCategoryFromFeed;

/// <summary>
/// Handles the <see cref="AdminUnpinCategoryFromFeedCommand" /> to remove a category from the
/// content feed. Idempotent: unpinning a category that is not pinned succeeds as a no-op.
/// </summary>
/// <param name="categoryRepository">Repository for category data access operations.</param>
/// <param name="unitOfWork">Unit of Work for managing database transactions.</param>
/// <param name="categoryDtoFactory">Builds category projections with their posters resolved.</param>
public class AdminUnpinCategoryFromFeedHandler(
    ICategoryRepository categoryRepository,
    IContentUnitOfWork unitOfWork,
    ICategoryDtoFactory categoryDtoFactory
) : ICommandHandler<AdminUnpinCategoryFromFeedCommand, AdminUnpinCategoryFromFeedResult>
{
    /// <inheritdoc />
    public async Task<AdminUnpinCategoryFromFeedResult> Handle(
        AdminUnpinCategoryFromFeedCommand command,
        CancellationToken cancellationToken
    )
    {
        Guid id = Guid.Parse(command.Id);

        CategoryEntity category = await categoryRepository.GetByIdOrThrowAsync(
            id: id,
            cancellationToken: cancellationToken
        );

        category.UnpinFromFeed();
        await unitOfWork.CommitAsync(cancellationToken: cancellationToken);

        CategoryEntity updated = await categoryRepository.GetByIdOrThrowAsync(
            id: id,
            cancellationToken: cancellationToken
        );

        var dto = await categoryDtoFactory.CreateAsync(updated, cancellationToken);
        return new AdminUnpinCategoryFromFeedResult(Category: dto);
    }
}
