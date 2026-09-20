using _116.Content.Application.Catalog.Factories;
using _116.Content.Application.Shared.DTOs;
using _116.Content.Application.Shared.Errors.Facade;
using _116.Content.Application.Shared.Mappers;
using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Catalog.UseCases.Admin.Commands.RemovePackageSlot;

/// <summary>
/// Handles the <see cref="AdminRemovePackageSlotCommand" /> to remove a slot from a package.
/// </summary>
/// <param name="packageRepository">Repository for package data access operations.</param>
/// <param name="unitOfWork">Unit of Work for managing database transactions.</param>
/// <param name="packageDtoFactory">Builds package projections with their categories resolved.</param>
/// <param name="i18n">Single i18n entry point for the Content module.</param>
public class AdminRemovePackageSlotHandler(
    IPackageRepository packageRepository,
    IContentUnitOfWork unitOfWork,
    IPackageDtoFactory packageDtoFactory,
    ContentI18n i18n
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

        PackageEntity package = await packageRepository.GetByIdOrThrowAsync(
            id: packageId,
            cancellationToken: cancellationToken
        );

        if (!package.RemoveSlot(slotId: slotId))
        {
            throw i18n.Package.SlotNotFound(slotId: slotId);
        }

        await unitOfWork.CommitAsync(cancellationToken: cancellationToken);

        PackageDto dto = await packageDtoFactory.CreateAsync(package, cancellationToken);
        return new AdminRemovePackageSlotResult(Package: dto, IsSuccess: true);
    }
}
