using _116.Content.Application.Shared.DTOs;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Catalog.UseCases.Admin.Commands.RemovePackageSlot;

/// <summary>
/// Command for removing a slot from a package.
/// </summary>
/// <param name="PackageId">The identifier of the package.</param>
/// <param name="SlotId">The identifier of the slot to remove.</param>
public record AdminRemovePackageSlotCommand(string PackageId, Guid SlotId) : ICommand<AdminRemovePackageSlotResult>;

/// <summary>
/// Result of the <see cref="AdminRemovePackageSlotCommand" /> containing the updated package.
/// </summary>
/// <param name="Package">The package with its remaining slots after removal.</param>
/// <param name="IsSuccess">Indicates whether the slot was successfully removed.</param>
public record AdminRemovePackageSlotResult(PackageDto Package, bool IsSuccess);
