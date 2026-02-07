using _116.Identity.Application.Shared.DTOs;
using _116.Identity.Application.Shared.Mappers;
using _116.Identity.Application.Shared.Repositories;
using _116.Identity.Domain.Entities;
using _116.Shared.Application.Pagination;
using _116.Shared.Contracts.Application.CQRS;
using MapsterMapper;

namespace _116.Identity.Application.Roles.UseCases.Admin.Queries.GetAllRoles;

/// <summary>
/// Handles the <see cref="AdminGetAllRolesQuery" /> to retrieve all roles with pagination and filtering.
/// </summary>
/// <param name="roleRepository">Repository for role data access operations.</param>
/// <param name="mapper">Mapster mapper for entity-to-DTO transformations.</param>
public class AdminGetAllRolesHandler(IRoleRepository roleRepository, IMapper mapper)
    : IQueryHandler<AdminGetAllRolesQuery, AdminGetAllRolesResult>
{
    /// <summary>
    /// Handles the query by fetching roles with pagination and optional filters.
    /// </summary>
    /// <param name="query">The query containing pagination and filter parameters.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A <see cref="AdminGetAllRolesResult" /> containing paginated role DTOs.</returns>
    public async Task<AdminGetAllRolesResult> Handle(AdminGetAllRolesQuery query, CancellationToken cancellationToken)
    {
        int pageSize = query.PaginatedRequest.PageSize;
        int pageIndex = query.PaginatedRequest.PageIndex;

        var (roles, totalCount) = await roleRepository.GetAllWithPaginationAsync(
            page: pageIndex + 1,
            pageSize: pageSize,
            search: query.Search,
            isActive: query.IsActive,
            isDeleted: query.IsDeleted,
            cancellationToken: cancellationToken
        );

        List<RoleDto> roleDtos = roles.Select(r => r.ToRoleDto(mapper)).ToList();

        var paginatedResult = new PaginatedResult<RoleDto>(
            pageIndex: pageIndex,
            pageSize: pageSize,
            count: totalCount,
            items: roleDtos
        );

        return new AdminGetAllRolesResult(Roles: paginatedResult);
    }
}
