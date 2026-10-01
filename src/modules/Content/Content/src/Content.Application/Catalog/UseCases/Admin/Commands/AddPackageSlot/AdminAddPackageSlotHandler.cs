using _116.BuildingBlocks.Application.CQRS;
using _116.Content.Application.Catalog.Services;
using _116.Content.Application.Catalog.UseCases.Admin.Commands.AddPackageSlot.Contracts;
using _116.Content.Application.Shared.DTOs;
using _116.Content.Application.Shared.Persistence;
using _116.Content.Domain.Entities;

namespace _116.Content.Application.Catalog.UseCases.Admin.Commands.AddPackageSlot;

/// <summary>
/// Handles the <see cref="AdminAddPackageSlotCommand" /> to add a slot to a package.
/// </summary>
/// <param name="addSlotService">Service resolving and applying the slot.</param>
/// <param name="unitOfWork">Unit of Work for managing database transactions.</param>
/// <param name="packageDtoService">Service assembling the package DTO.</param>
public class AdminAddPackageSlotHandler(
    IAdminAddPackageSlotService addSlotService,
    IContentUnitOfWork unitOfWork,
    IPackageDtoService packageDtoService
) : ICommandHandler<AdminAddPackageSlotCommand, AdminAddPackageSlotResult>
{
    /// <inheritdoc />
    public async Task<AdminAddPackageSlotResult> Handle(
        AdminAddPackageSlotCommand command,
        CancellationToken cancellationToken
    )
    {
        PackageEntity package = await addSlotService.AddSlotAsync(
            packageId: Guid.Parse(command.PackageId),
            categoryId: command.CategoryId,
            isRequired: command.IsRequired,
            quantity: command.Quantity,
            cancellationToken: cancellationToken
        );

        await unitOfWork.CommitAsync(cancellationToken: cancellationToken);

        PackageDto dto = await packageDtoService.CreateAsync(package, cancellationToken);
        return new AdminAddPackageSlotResult(Package: dto);
    }
}
