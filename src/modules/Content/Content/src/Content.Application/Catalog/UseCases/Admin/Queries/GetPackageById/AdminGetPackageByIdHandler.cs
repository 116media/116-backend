using _116.BuildingBlocks.Application.CQRS;
using _116.Content.Application.Catalog.Services;
using _116.Content.Application.Shared.DTOs;
using _116.Content.Application.Shared.Mappers;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;

namespace _116.Content.Application.Catalog.UseCases.Admin.Queries.GetPackageById;

/// <summary>
/// Handles the <see cref="AdminGetPackageByIdQuery" /> to retrieve a package by its identifier.
/// </summary>
/// <param name="packageRepository">Repository for package data access operations.</param>
/// <param name="packageDtoService">Builds package projections with their categories resolved.</param>
public class AdminGetPackageByIdHandler(IPackageRepository packageRepository, IPackageDtoService packageDtoService)
    : IQueryHandler<AdminGetPackageByIdQuery, AdminGetPackageByIdResult>
{
    /// <inheritdoc />
    public async Task<AdminGetPackageByIdResult> Handle(
        AdminGetPackageByIdQuery query,
        CancellationToken cancellationToken
    )
    {
        PackageEntity package = await packageRepository.GetByIdOrThrowAsync(
            id: query.Id,
            cancellationToken: cancellationToken
        );

        PackageDto dto = await packageDtoService.CreateAsync(package, cancellationToken);
        return new AdminGetPackageByIdResult(Package: dto);
    }
}
