using _116.Content.Application.Shared.DTOs;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Catalog.UseCases.Admin.Commands.AddPackageSlot;

/// <summary>
/// Command for adding a new slot to an existing package.
/// </summary>
/// <param name="PackageId">The unique identifier of the package to add the slot to.</param>
/// <param name="CategoryId">The optional category for this slot. Null means an open slot where the client may choose any category.</param>
/// <param name="IsRequired">Whether this slot must be fulfilled for the package to be considered complete.</param>
/// <param name="Quantity">The number of content pieces required for this slot (must be > 0).</param>
public record AdminAddPackageSlotCommand(string PackageId, Guid? CategoryId, bool IsRequired, int Quantity)
    : ICommand<AdminAddPackageSlotResult>;

/// <summary>
/// Result of the <see cref="AdminAddPackageSlotCommand" /> containing the updated package details.
/// </summary>
/// <param name="Package">The updated package information including the new slot.</param>
public record AdminAddPackageSlotResult(PackageDto Package);
