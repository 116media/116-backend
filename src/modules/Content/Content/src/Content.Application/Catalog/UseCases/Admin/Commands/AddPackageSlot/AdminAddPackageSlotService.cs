using _116.Content.Application.Catalog.UseCases.Admin.Commands.AddPackageSlot.Contracts;
using _116.Content.Application.Shared.Errors;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;

namespace _116.Content.Application.Catalog.UseCases.Admin.Commands.AddPackageSlot;

/// <summary>
/// Resolves and applies a package slot for the admin add-slot use case.
/// </summary>
/// <param name="packageRepository">Repository loading the package aggregate.</param>
/// <param name="categoryRepository">Repository validating the slot's category.</param>
public class AdminAddPackageSlotService(IPackageRepository packageRepository, ICategoryRepository categoryRepository)
    : IAdminAddPackageSlotService
{
    /// <inheritdoc />
    public async Task<PackageEntity> AddSlotAsync(
        Guid packageId,
        Guid? categoryId,
        bool isRequired,
        int quantity,
        CancellationToken cancellationToken
    )
    {
        PackageEntity package = await packageRepository.GetByIdOrThrowAsync(
            id: packageId,
            cancellationToken: cancellationToken
        );

        if (categoryId is { } id)
        {
            CategoryEntity? category = await categoryRepository.GetByIdAsync(
                id: id,
                cancellationToken: cancellationToken
            );

            if (category is null)
            {
                throw CategoryErrors.NotFound(id: id);
            }
        }

        package.AddSlot(categoryId: categoryId, isRequired: isRequired, quantity: quantity);

        return package;
    }
}
