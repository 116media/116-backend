using _116.Identity.Application.Shared.DTOs;
using _116.Identity.Application.Shared.Mappers;
using _116.Identity.Application.Shared.Repositories;
using _116.Identity.Domain.Entities;
using _116.Shared.Contracts.Application.CQRS;
using MapsterMapper;

namespace _116.Identity.Application.Roles.UseCases.Admin.Queries.GetOwnRoles;

/// <summary>
/// Handles the <see cref="AdminGetOwnRolesQuery" /> to retrieve the authenticated admin's roles with permissions.
/// </summary>
/// <param name="authRepository">Repository for user data access and authentication operations.</param>
/// <param name="mapper">Mapster mapper for entity-to-DTO transformations.</param>
public class AdminGetOwnRolesHandler(IAuthRepository authRepository, IMapper mapper)
    : IQueryHandler<AdminGetOwnRolesQuery, AdminGetOwnRolesResult>
{
    /// <inheritdoc />
    public async Task<AdminGetOwnRolesResult> Handle(AdminGetOwnRolesQuery query, CancellationToken cancellationToken)
    {
        UserEntity? user = await authRepository.GetUserWithRolesAndPermissionsByIdOrThrow(
            userId: query.UserId,
            cancellationToken: cancellationToken
        );

        IReadOnlyList<RoleWithPermissionsDto> roles = user!
            .UserRoles.Select(ur => ur.Role.ToRoleWithPermissionsDto(mapper))
            .ToList();

        return new AdminGetOwnRolesResult(Roles: roles);
    }
}
