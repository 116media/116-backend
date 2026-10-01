using _116.Identity.Domain.Entities;

namespace _116.Identity.Application.User.UseCases.Admin.Commands.AssignRoleToUser.Contracts;

/// <summary>
/// The user carrying the fresh grant and the role that was granted.
/// </summary>
/// <param name="User">The user aggregate with the grant applied.</param>
/// <param name="Role">The granted role.</param>
public record RoleGrantData(UserEntity User, RoleEntity Role);

/// <summary>
/// Resolves and applies a role grant: loads the role and the user, gates deleted and inactive
/// roles, and grants through the user aggregate.
/// </summary>
public interface IAdminAssignRoleToUserService
{
    /// <summary>
    /// Grants the role to the user, throwing the localized error when the role is deleted,
    /// inactive, or already assigned. The caller owns the commit.
    /// </summary>
    /// <param name="userId">The user receiving the role.</param>
    /// <param name="roleId">The role being granted.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>The user with the grant applied and the granted role.</returns>
    Task<RoleGrantData> GrantAsync(Guid userId, Guid roleId, CancellationToken cancellationToken);
}
