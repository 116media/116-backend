using _116.Content.Application.Shared.DTOs;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Catalog.UseCases.Admin.Commands.DeactivatePackage;

/// <summary>
/// Command to deactivate a package, removing it from available bundles for new orders.
/// </summary>
/// <param name="Id">The unique identifier of the package to deactivate.</param>
public record DeactivatePackageCommand(Guid Id) : ICommand<DeactivatePackageResult>;

/// <summary>
/// Result returned after successfully deactivating a package.
/// </summary>
/// <param name="Package">The updated package information.</param>
public record DeactivatePackageResult(PackageDto Package);
