using _116.Content.Application.Catalog.Factories;
using _116.Content.Application.Shared.DTOs;
using _116.Content.Application.Shared.Mappers;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Shared.Application.Pagination;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Catalog.UseCases.Admin.Queries.GetAllPackages;

/// <summary>
/// Handles the <see cref="AdminGetAllPackagesQuery" /> to retrieve a paginated list of packages.
/// </summary>
/// <param name="packageRepository">Repository for package data access operations.</param>
/// <param name="packageDtoFactory">Builds package projections with their categories resolved.</param>
public class AdminGetAllPackagesHandler(IPackageRepository packageRepository, IPackageDtoFactory packageDtoFactory)
    : IQueryHandler<AdminGetAllPackagesQuery, AdminGetAllPackagesResult>
{
    /// <inheritdoc />
    public async Task<AdminGetAllPackagesResult> Handle(
        AdminGetAllPackagesQuery query,
        CancellationToken cancellationToken
    )
    {
        int pageSize = query.PaginatedRequest.PageSize;
        int pageIndex = query.PaginatedRequest.PageIndex;

        (List<PackageEntity> packages, int totalCount) = await packageRepository.GetAllAsync(
            page: pageIndex + 1,
            pageSize: pageSize,
            isActive: query.IsActive,
            cancellationToken: cancellationToken
        );

        IReadOnlyList<PackageDto> dtoList = await packageDtoFactory.CreateManyAsync(packages, cancellationToken);

        var paginatedResult = new PaginatedResult<PackageDto>(
            pageIndex: pageIndex,
            pageSize: pageSize,
            count: totalCount,
            items: dtoList
        );

        return new AdminGetAllPackagesResult(Packages: paginatedResult);
    }
}
