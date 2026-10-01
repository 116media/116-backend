using _116.Identity.Domain.Entities;

namespace _116.Identity.Application.User.UseCases.Admin.Commands.RemoveRoleFromUser.Contracts;

/// <summary>
/// Resolves and applies a role revocation: loads the role and the user, and revokes through the
/// user aggregate.
/// </summary>
public interface IAdminRemoveRoleFromUserService
{
    /// <summary>
    /// Revokes the role from the user, throwing the localized error when the role is not
    /// assigned. The caller owns the commit.
    /// </summary>
    /// <param name="userId">The user losing the role.</param>
    /// <param name="roleId">The role being revoked.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>The user with the revocation applied.</returns>
    Task<UserEntity> RevokeAsync(Guid userId, Guid roleId, CancellationToken cancellationToken);
}
