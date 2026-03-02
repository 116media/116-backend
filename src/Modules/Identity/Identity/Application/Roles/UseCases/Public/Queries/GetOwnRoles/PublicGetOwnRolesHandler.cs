using _116.Identity.Application.Shared.DTOs;
using _116.Identity.Application.Shared.Mappers;
using _116.Identity.Application.Shared.Repositories;
using _116.Identity.Domain.Entities;
using _116.Shared.Contracts.Application.CQRS;
using MapsterMapper;

namespace _116.Identity.Application.Roles.UseCases.Public.Queries.GetOwnRoles;

/// <summary>
/// Handles the <see cref="PublicGetOwnRolesQuery" /> to retrieve the authenticated user's roles with permissions.
/// </summary>
/// <param name="authRepository">Repository for user data access and authentication operations.</param>
/// <param name="mapper">Mapster mapper for entity-to-DTO transformations.</param>
public class PublicGetOwnRolesHandler(IAuthRepository authRepository, IMapper mapper)
    : IQueryHandler<PublicGetOwnRolesQuery, PublicGetOwnRolesResult>
{
    /// <inheritdoc />
    public async Task<PublicGetOwnRolesResult> Handle(PublicGetOwnRolesQuery query, CancellationToken cancellationToken)
    {
        UserEntity? user = await authRepository.GetUserWithRolesAndPermissionsByIdOrThrow(
            userId: query.UserId,
            cancellationToken: cancellationToken
        );

        IReadOnlyList<RoleWithPermissionsDto> roles = user!
            .UserRoles.Select(ur => ur.Role.ToRoleWithPermissionsDto(mapper))
            .ToList();

        return new PublicGetOwnRolesResult(Roles: roles);
    }
}
