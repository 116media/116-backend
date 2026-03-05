using _116.Content.Application.Shared.DTOs;
using _116.Content.Application.Shared.Mappers;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Shared.Application.Pagination;
using _116.Shared.Contracts.Application.CQRS;
using MapsterMapper;

namespace _116.Content.Application.Catalog.UseCases.Admin.Queries.GetAllPackages;

/// <summary>
/// Handles the <see cref="GetAllPackagesQuery" /> to retrieve a paginated list of packages.
/// </summary>
/// <param name="packageRepository">Repository for package data access operations.</param>
/// <param name="mapper">Mapster mapper for entity-to-DTO transformations.</param>
public class GetAllPackagesHandler(IPackageRepository packageRepository, IMapper mapper)
    : IQueryHandler<GetAllPackagesQuery, GetAllPackagesResult>
{
    /// <inheritdoc />
    public async Task<GetAllPackagesResult> Handle(GetAllPackagesQuery query, CancellationToken cancellationToken)
    {
        int pageSize = query.PaginatedRequest.PageSize;
        int pageIndex = query.PaginatedRequest.PageIndex;

        (List<PackageEntity> packages, int totalCount) = await packageRepository.GetAllAsync(
            page: pageIndex + 1,
            pageSize: pageSize,
            isActive: query.IsActive,
            cancellationToken: cancellationToken
        );

        List<PackageDto> dtoList = packages.Select(p => p.ToPackageDto(mapper)).ToList();

        var paginatedResult = new PaginatedResult<PackageDto>(
            pageIndex: pageIndex,
            pageSize: pageSize,
            count: totalCount,
            items: dtoList
        );

        return new GetAllPackagesResult(Packages: paginatedResult);
    }
}
