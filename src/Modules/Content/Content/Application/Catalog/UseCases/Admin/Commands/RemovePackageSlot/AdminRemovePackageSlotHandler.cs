using _116.Content.Application.Shared.DTOs;
using _116.Content.Application.Shared.Errors;
using _116.Content.Application.Shared.Mappers;
using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Shared.Contracts.Application.CQRS;
using MapsterMapper;

namespace _116.Content.Application.Catalog.UseCases.Admin.Commands.RemovePackageSlot;

/// <summary>
/// Handles the <see cref="AdminRemovePackageSlotCommand" /> to remove a slot from a package.
/// </summary>
/// <param name="packageRepository">Repository for package data access operations.</param>
/// <param name="unitOfWork">Unit of Work for managing database transactions.</param>
/// <param name="mapper">Mapster mapper for entity-to-DTO transformations.</param>
public class AdminRemovePackageSlotHandler(
    IPackageRepository packageRepository,
    IContentUnitOfWork unitOfWork,
    IMapper mapper
) : ICommandHandler<AdminRemovePackageSlotCommand, AdminRemovePackageSlotResult>
{
    /// <inheritdoc />
    public async Task<AdminRemovePackageSlotResult> Handle(
        AdminRemovePackageSlotCommand command,
        CancellationToken cancellationToken
    )
    {
        Guid packageId = Guid.Parse(command.PackageId);
        Guid slotId = Guid.Parse(command.SlotId);

        await packageRepository.GetByIdWithSlotsOrThrowAsync(id: packageId, cancellationToken: cancellationToken);

        PackageSlotEntity? slot = await packageRepository.GetSlotByIdAsync(
            slotId: slotId,
            cancellationToken: cancellationToken
        );

        if (slot is null)
        {
            throw PackageErrors.SlotNotFound(slotId: slotId);
        }

        packageRepository.RemoveSlot(slot: slot);
        await unitOfWork.CommitAsync(cancellationToken: cancellationToken);

        PackageEntity updatedPackage = await packageRepository.GetByIdWithSlotsOrThrowAsync(
            id: packageId,
            cancellationToken: cancellationToken
        );

        var dto = updatedPackage.ToPackageDto(mapper);
        return new AdminRemovePackageSlotResult(Package: dto, IsSuccess: true);
    }
}
