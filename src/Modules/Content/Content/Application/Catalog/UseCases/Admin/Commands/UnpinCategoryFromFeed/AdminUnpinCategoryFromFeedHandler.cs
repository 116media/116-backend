using _116.Content.Application.Shared.Mappers;
using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Core.Contracts.Application.Services;
using _116.Shared.Contracts.Application.CQRS;
using MapsterMapper;

namespace _116.Content.Application.Catalog.UseCases.Admin.Commands.UnpinCategoryFromFeed;

/// <summary>
/// Handles the <see cref="AdminUnpinCategoryFromFeedCommand" /> to remove a category from the
/// content feed. Idempotent: unpinning a category that is not pinned succeeds as a no-op.
/// </summary>
/// <param name="categoryRepository">Repository for category data access operations.</param>
/// <param name="unitOfWork">Unit of Work for managing database transactions.</param>
/// <param name="fileStorage">Core's storage contract.</param>
/// <param name="mapper">Mapster mapper for entity-to-DTO transformations.</param>
public class AdminUnpinCategoryFromFeedHandler(
    ICategoryRepository categoryRepository,
    IContentUnitOfWork unitOfWork,
    IFileStorageService fileStorage,
    IMapper mapper
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

        categoryRepository.Update(category: category);

        await unitOfWork.CommitAsync(cancellationToken: cancellationToken);

        CategoryEntity updated = await categoryRepository.GetByIdOrThrowAsync(
            id: id,
            cancellationToken: cancellationToken
        );

        var dto = await updated.ToCategoryDtoAsync(mapper, fileStorage, cancellationToken);
        return new AdminUnpinCategoryFromFeedResult(Category: dto);
    }
}
