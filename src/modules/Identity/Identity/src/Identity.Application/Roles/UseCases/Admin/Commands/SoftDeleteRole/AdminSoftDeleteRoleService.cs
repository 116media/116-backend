using _116.Identity.Application.Roles.UseCases.Admin.Commands.SoftDeleteRole.Contracts;
using _116.Identity.Application.Shared.Errors.Facade;
using _116.Identity.Application.Shared.Repositories;
using _116.Identity.Domain.Entities;

namespace _116.Identity.Application.Roles.UseCases.Admin.Commands.SoftDeleteRole;

/// <summary>
/// Loads the role and applies the clocked soft deletion for the admin soft-delete use case.
/// </summary>
/// <param name="roleRepository">Repository for role data access operations.</param>
/// <param name="i18n">Single i18n entry point for the Identity module.</param>
/// <param name="timeProvider">Clock supplying the deletion instant.</param>
public class AdminSoftDeleteRoleService(IRoleRepository roleRepository, IdentityI18n i18n, TimeProvider timeProvider)
    : IAdminSoftDeleteRoleService
{
    /// <inheritdoc />
    public async Task<RoleEntity> SoftDeleteAsync(Guid roleId, CancellationToken cancellationToken)
    {
        RoleEntity? role = await roleRepository.GetRoleByIdOrThrowAsync(
            roleId: roleId,
            cancellationToken: cancellationToken
        );

        if (!role!.SoftDelete(now: timeProvider.GetUtcNow().UtcDateTime))
        {
            throw i18n.User.RoleAlreadyDeleted();
        }

        return role;
    }
}
