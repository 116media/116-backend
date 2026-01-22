using _116.Identity.Application.Shared.DTOs;
using _116.Identity.Application.Shared.Mappers;
using _116.Identity.Application.Shared.Repositories;
using _116.Identity.Domain.Entities;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Identity.Application.Roles.UseCases.Admin.Queries.GetRoleById;

/// <summary>
/// Handles the <see cref="AdminGetRoleByIdQuery" /> to retrieve a role by its ID with associated permissions.
/// </summary>
/// <param name="roleRepository">Repository for role data access operations.</param>
public class AdminGetRoleByIdHandler(IRoleRepository roleRepository)
    : IQueryHandler<AdminGetRoleByIdQuery, AdminGetRoleByIdResult>
{
    /// <summary>
    /// Handles the query by fetching the role with its permissions.
    /// </summary>
    /// <param name="query">The query containing the role ID.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A <see cref="AdminGetRoleByIdResult" /> containing the role and its permissions.</returns>
    public async Task<AdminGetRoleByIdResult> Handle(AdminGetRoleByIdQuery query, CancellationToken cancellationToken)
    {
        Guid roleId = Guid.Parse(input: query.RoleId);

        RoleEntity? role = await roleRepository.GetRoleByIdWithPermissionsOrThrowAsync(
            roleId: roleId,
            cancellationToken: cancellationToken
        );

        var roleDto = role!.ToRoleDto();
        IReadOnlyCollection<PermissionDto> permissions = role!.RolePermissions.ToPermissionDtos();

        return new AdminGetRoleByIdResult(Role: roleDto, Permissions: permissions);
    }
}
