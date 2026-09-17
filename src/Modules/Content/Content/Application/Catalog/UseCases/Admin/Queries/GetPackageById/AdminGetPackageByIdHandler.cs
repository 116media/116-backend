using _116.Content.Application.Catalog.Factories;
using _116.Content.Application.Shared.DTOs;
using _116.Content.Application.Shared.Mappers;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Catalog.UseCases.Admin.Queries.GetPackageById;

/// <summary>
/// Handles the <see cref="AdminGetPackageByIdQuery" /> to retrieve a package by its identifier.
/// </summary>
/// <param name="packageRepository">Repository for package data access operations.</param>
/// <param name="packageDtoFactory">Builds package projections with their categories resolved.</param>
public class AdminGetPackageByIdHandler(IPackageRepository packageRepository, IPackageDtoFactory packageDtoFactory)
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

        PackageDto dto = await packageDtoFactory.CreateAsync(package, cancellationToken);
        return new AdminGetPackageByIdResult(Package: dto);
    }
}
