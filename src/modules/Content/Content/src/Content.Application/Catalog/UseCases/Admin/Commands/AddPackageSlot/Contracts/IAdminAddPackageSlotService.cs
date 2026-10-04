using _116.Content.Domain.Entities;

namespace _116.Content.Application.Catalog.UseCases.Admin.Commands.AddPackageSlot.Contracts;

/// <summary>
/// Resolves and applies a package slot: loads the package, validates the optional category, and
/// adds the slot through the package aggregate.
/// </summary>
public interface IAdminAddPackageSlotService
{
    /// <summary>
    /// Adds the slot, throwing the localized error when the category does not exist. The caller
    /// owns the commit.
    /// </summary>
    /// <param name="packageId">The package receiving the slot.</param>
    /// <param name="categoryId">The category the slot is for, or null for a free slot.</param>
    /// <param name="isRequired">Whether the slot is required.</param>
    /// <param name="quantity">How many items the slot holds.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>The package with the slot added.</returns>
    Task<PackageEntity> AddSlotAsync(
        Guid packageId,
        Guid? categoryId,
        bool isRequired,
        int quantity,
        CancellationToken cancellationToken
    );
}
