using _116.Content.Application.Shared.Errors;
using _116.Content.Application.Shared.Mappers;
using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Shared.Contracts.Application.CQRS;
using MapsterMapper;

namespace _116.Content.Application.Catalog.UseCases.Admin.Commands.AddPackageSlot;

/// <summary>
/// Handles the <see cref="AddPackageSlotCommand" /> to add a new slot to an existing package.
/// </summary>
/// <param name="packageRepository">Repository for package data access operations.</param>
/// <param name="categoryRepository">Repository for verifying category existence.</param>
/// <param name="unitOfWork">Unit of Work for managing database transactions.</param>
/// <param name="mapper">Mapster mapper for entity-to-DTO transformations.</param>
public class AddPackageSlotHandler(
    IPackageRepository packageRepository,
    ICategoryRepository categoryRepository,
    IContentUnitOfWork unitOfWork,
    IMapper mapper
) : ICommandHandler<AddPackageSlotCommand, AddPackageSlotResult>
{
    /// <inheritdoc />
    public async Task<AddPackageSlotResult> Handle(AddPackageSlotCommand command, CancellationToken cancellationToken)
    {
        Guid packageId = Guid.Parse(command.PackageId);

        await packageRepository.GetByIdWithSlotsOrThrowAsync(id: packageId, cancellationToken: cancellationToken);

        if (command.CategoryId.HasValue)
        {
            CategoryEntity? category = await categoryRepository.GetByIdAsync(
                id: command.CategoryId.Value,
                cancellationToken: cancellationToken
            );

            if (category is null)
            {
                throw CategoryErrors.NotFound(id: command.CategoryId.Value);
            }
        }

        var slot = PackageSlotEntity.Create(
            id: Guid.NewGuid(),
            packageId: packageId,
            categoryId: command.CategoryId,
            isRequired: command.IsRequired,
            quantity: command.Quantity
        );

        await packageRepository.AddSlotAsync(slot: slot, cancellationToken: cancellationToken);
        await unitOfWork.CommitAsync(cancellationToken: cancellationToken);

        PackageEntity updated = await packageRepository.GetByIdWithSlotsOrThrowAsync(
            id: packageId,
            cancellationToken: cancellationToken
        );

        var dto = updated.ToPackageDto(mapper);
        return new AddPackageSlotResult(Package: dto);
    }
}
