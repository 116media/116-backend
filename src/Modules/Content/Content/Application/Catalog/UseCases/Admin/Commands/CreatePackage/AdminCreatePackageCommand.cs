using _116.Content.Application.Shared.DTOs;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Catalog.UseCases.Admin.Commands.CreatePackage;

/// <summary>
/// Command for creating a new content package.
/// Price is derived from the required slots' category tier prices.
/// </summary>
/// <param name="Name">The display name of the package.</param>
/// <param name="Description">The description of what the package includes.</param>
public record AdminCreatePackageCommand(string Name, string Description) : ICommand<AdminCreatePackageResult>;

/// <summary>
/// Result of the <see cref="AdminCreatePackageCommand" /> containing the created package details.
/// </summary>
/// <param name="Package">The created package information.</param>
public record AdminCreatePackageResult(PackageDto Package);
