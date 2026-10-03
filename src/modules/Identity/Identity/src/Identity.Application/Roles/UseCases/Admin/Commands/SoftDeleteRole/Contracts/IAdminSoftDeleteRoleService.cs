using _116.Identity.Domain.Entities;

namespace _116.Identity.Application.Roles.UseCases.Admin.Commands.SoftDeleteRole.Contracts;

/// <summary>
/// Loads a role and applies the clocked soft deletion.
/// </summary>
public interface IAdminSoftDeleteRoleService
{
    /// <summary>
    /// Soft-deletes the role, throwing the localized error when it is already deleted. The
    /// caller owns the commit.
    /// </summary>
    /// <param name="roleId">The role being deleted.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>The role with the deletion applied.</returns>
    Task<RoleEntity> SoftDeleteAsync(Guid roleId, CancellationToken cancellationToken);
}
