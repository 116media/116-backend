using _116.Identity.Application.Roles.Specifications;
using _116.Identity.Application.Shared.Repositories;
using _116.Identity.Domain.Entities;
using _116.Identity.Infrastructure.Persistence;
using _116.Shared.Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;

namespace _116.Identity.Infrastructure.Repositories;

/// <summary>
/// Implementation of <see cref="IRolePermissionRepository" /> for managing role-permission associations.
/// </summary>
/// <param name="context">The database context for accessing role-permission data.</param>
public class RolePermissionRepository(IdentityDbContext context) : IRolePermissionRepository
{
    /// <inheritdoc />
    public async Task<bool> ExistsByRoleAndPermissionAsync(
        Guid roleId,
        Guid permissionId,
        CancellationToken cancellationToken = default
    )
    {
        var specification = new RolePermissionByRoleAndPermissionSpecification(
            roleId: roleId,
            permissionId: permissionId
        );
        return await context
            .RolePermissions.ApplySpecification(specification: specification)
            .AnyAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<RolePermissionEntity?> GetByRoleAndPermissionAsync(
        Guid roleId,
        Guid permissionId,
        CancellationToken cancellationToken = default
    )
    {
        var specification = new RolePermissionByRoleAndPermissionSpecification(
            roleId: roleId,
            permissionId: permissionId
        );
        return await context
            .RolePermissions.ApplySpecification(specification: specification)
            .FirstOrDefaultAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task AddAsync(RolePermissionEntity entity, CancellationToken cancellationToken = default)
    {
        await context.RolePermissions.AddAsync(entity, cancellationToken);
    }

    /// <inheritdoc />
    public void Delete(RolePermissionEntity entity)
    {
        context.RolePermissions.Remove(entity);
    }

    /// <inheritdoc />
    public async Task<List<Guid>> GetPermissionIdsByRoleIdAsync(
        Guid roleId,
        CancellationToken cancellationToken = default
    )
    {
        var specification = new RolePermissionByRoleIdSpecification(roleId: roleId);
        return await context
            .RolePermissions.ApplySpecification(specification: specification)
            .Select(rp => rp.PermissionId)
            .ToListAsync(cancellationToken);
    }
}
