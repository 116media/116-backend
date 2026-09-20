using _116.Content.Application.Catalog.Factories;
using _116.Content.Application.Shared.DTOs;
using _116.Content.Application.Shared.Errors.Facade;
using _116.Content.Application.Shared.Mappers;
using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Catalog.UseCases.Admin.Commands.AddPackageSlot;

/// <summary>
/// Handles the <see cref="AdminAddPackageSlotCommand" /> to add a new slot to an existing package.
/// </summary>
/// <param name="packageRepository">Repository for package data access operations.</param>
/// <param name="categoryRepository">Repository for verifying category existence.</param>
/// <param name="unitOfWork">Unit of Work for managing database transactions.</param>
/// <param name="packageDtoFactory">Builds package projections with their categories resolved.</param>
/// <param name="i18n">Single i18n entry point for the Content module.</param>
public class AdminAddPackageSlotHandler(
    IPackageRepository packageRepository,
    ICategoryRepository categoryRepository,
    IContentUnitOfWork unitOfWork,
    IPackageDtoFactory packageDtoFactory,
    ContentI18n i18n
) : ICommandHandler<AdminAddPackageSlotCommand, AdminAddPackageSlotResult>
{
    /// <inheritdoc />
    public async Task<AdminAddPackageSlotResult> Handle(
        AdminAddPackageSlotCommand command,
        CancellationToken cancellationToken
    )
    {
        Guid packageId = Guid.Parse(command.PackageId);

        PackageEntity package = await packageRepository.GetByIdOrThrowAsync(
            id: packageId,
            cancellationToken: cancellationToken
        );

        if (command.CategoryId.HasValue)
        {
            CategoryEntity? category = await categoryRepository.GetByIdAsync(
                id: command.CategoryId.Value,
                cancellationToken: cancellationToken
            );

            if (category is null)
            {
                throw i18n.Category.NotFound(id: command.CategoryId.Value);
            }
        }

        package.AddSlot(categoryId: command.CategoryId, isRequired: command.IsRequired, quantity: command.Quantity);
        await unitOfWork.CommitAsync(cancellationToken: cancellationToken);

        PackageDto dto = await packageDtoFactory.CreateAsync(package, cancellationToken);
        return new AdminAddPackageSlotResult(Package: dto);
    }
}
