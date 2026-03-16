using _116.Identity.Application.Shared.DTOs;
using _116.Identity.Application.Shared.Mappers;
using _116.Identity.Application.Shared.Repositories;
using _116.Identity.Domain.Entities;
using _116.Shared.Contracts.Application.CQRS;
using MapsterMapper;

namespace _116.Identity.Application.User.UseCases.Admin.Queries.GetUserRoles;

/// <summary>
/// Handles the <see cref="AdminGetUserRolesQuery" /> to get all roles assigned to a user.
/// </summary>
/// <param name="userRoleRepository">Repository for user-role data access operations.</param>
/// <param name="mapper">Mapster mapper for entity-to-DTO transformations.</param>
public class AdminGetUserRolesHandler(IUserRoleRepository userRoleRepository, IMapper mapper)
    : IQueryHandler<AdminGetUserRolesQuery, AdminGetUserRolesResult>
{
    /// <summary>
    /// Handles the get user roles query.
    /// </summary>
    /// <param name="query">The query containing the user ID.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A <see cref="AdminGetUserRolesResult" /> containing the user's roles.</returns>
    public async Task<AdminGetUserRolesResult> Handle(AdminGetUserRolesQuery query, CancellationToken cancellationToken)
    {
        Guid userId = Guid.Parse(input: query.UserId);

        List<UserRoleEntity> userRoles = await userRoleRepository.GetUserRolesWithRoleAsync(
            userId: userId,
            cancellationToken: cancellationToken
        );

        IReadOnlyCollection<RoleDto> roles = userRoles.Select(ur => ur.Role.ToRoleDto(mapper)).ToList();
        return new AdminGetUserRolesResult(Roles: roles);
    }
}
