using _116.Content.Application.Shared.DTOs;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Catalog.UseCases.Admin.Commands.ActivatePackage;

/// <summary>
/// Command to activate a package, making it available for new orders.
/// </summary>
/// <param name="Id">The unique identifier of the package to activate.</param>
public record ActivatePackageCommand(Guid Id) : ICommand<ActivatePackageResult>;

/// <summary>
/// Result returned after successfully activating a package.
/// </summary>
/// <param name="Package">The updated package information.</param>
public record ActivatePackageResult(PackageDto Package);
