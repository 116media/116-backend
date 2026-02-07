using _116.Identity.Application.Shared.DTOs;
using _116.Identity.Application.Shared.Mappers;
using _116.Identity.Application.Shared.Repositories;
using _116.Identity.Domain.Entities;
using _116.Shared.Application.Pagination;
using _116.Shared.Contracts.Application.CQRS;
using MapsterMapper;

namespace _116.Identity.Application.Roles.UseCases.Admin.Queries.GetAllPermissions;

/// <summary>
/// Handles the <see cref="AdminGetAllPermissionsQuery" /> to retrieve all permissions with pagination and filtering.
/// </summary>
/// <param name="permissionRepository">Repository for permission data access operations.</param>
/// <param name="mapper">Mapster mapper for entity-to-DTO transformations.</param>
public class AdminGetAllPermissionsHandler(IPermissionRepository permissionRepository, IMapper mapper)
    : IQueryHandler<AdminGetAllPermissionsQuery, AdminGetAllPermissionsResult>
{
    /// <summary>
    /// Handles the query by fetching permissions with pagination and optional filters.
    /// </summary>
    /// <param name="query">The query containing pagination and filter parameters.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A <see cref="AdminGetAllPermissionsResult" /> containing paginated permission DTOs.</returns>
    public async Task<AdminGetAllPermissionsResult> Handle(
        AdminGetAllPermissionsQuery query,
        CancellationToken cancellationToken
    )
    {
        int pageSize = query.PaginatedRequest.PageSize;
        int pageIndex = query.PaginatedRequest.PageIndex;

        var (permissions, totalCount) = await permissionRepository.GetAllWithPaginationAsync(
            page: pageIndex + 1,
            pageSize: pageSize,
            search: query.Search,
            isActive: query.IsActive,
            isDeleted: query.IsDeleted,
            cancellationToken: cancellationToken
        );

        List<PermissionDto> permissionDtos = permissions.Select(p => p.ToPermissionDto(mapper)).ToList();

        var paginatedResult = new PaginatedResult<PermissionDto>(
            pageIndex: pageIndex,
            pageSize: pageSize,
            count: totalCount,
            items: permissionDtos
        );

        return new AdminGetAllPermissionsResult(Permissions: paginatedResult);
    }
}
