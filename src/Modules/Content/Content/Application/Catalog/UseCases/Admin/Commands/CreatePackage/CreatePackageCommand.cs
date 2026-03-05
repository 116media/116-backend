using _116.Content.Application.Shared.DTOs;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Catalog.UseCases.Admin.Commands.CreatePackage;

/// <summary>
/// Command for creating a new content package with a flat price.
/// </summary>
/// <param name="Name">The display name of the package.</param>
/// <param name="Description">An optional description of what the package includes.</param>
/// <param name="FlatPriceUsd">The flat price in USD for the entire package (must be >= 0).</param>
public record CreatePackageCommand(string Name, string? Description, decimal FlatPriceUsd)
    : ICommand<CreatePackageResult>;

/// <summary>
/// Result of the <see cref="CreatePackageCommand" /> containing the created package details.
/// </summary>
/// <param name="Package">The created package information.</param>
public record CreatePackageResult(PackageDto Package);
