using _116.BuildingBlocks.Application.CQRS;
using _116.Content.Application.Shared.DTOs;

namespace _116.Content.Application.Catalog.UseCases.Admin.Queries.GetPackageById;

/// <summary>
/// Query for retrieving a single package by its identifier, including its slot composition.
/// </summary>
/// <param name="Id">The unique identifier of the package.</param>
public record AdminGetPackageByIdQuery(Guid Id) : IQuery<AdminGetPackageByIdResult>;

/// <summary>
/// Result of the <see cref="AdminGetPackageByIdQuery" /> containing the package details.
/// </summary>
/// <param name="Package">The package information including all slots.</param>
public record AdminGetPackageByIdResult(PackageDto Package);
