using _116.Identity.Application.Roles.Specifications;
using _116.Identity.Application.Shared.Repositories;
using _116.Identity.Domain.Entities;
using _116.Identity.Infrastructure.Persistence;
using _116.Shared.Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;

namespace _116.Identity.Infrastructure.Repositories;

/// <summary>
/// Implementation of <see cref="IUserRoleRepository" /> for managing user-role associations.
/// </summary>
/// <param name="context">The database context for accessing user-role data.</param>
public class UserRoleRepository(IdentityDbContext context) : IUserRoleRepository
{
    /// <inheritdoc />
    public async Task<bool> ExistsByUserAndRoleAsync(
        Guid userId,
        Guid roleId,
        CancellationToken cancellationToken = default
    )
    {
        var specification = new UserRoleByUserAndRoleSpecification(userId: userId, roleId: roleId);
        return await context.UserRoles.ApplySpecification(specification: specification).AnyAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<UserRoleEntity?> GetByUserAndRoleAsync(
        Guid userId,
        Guid roleId,
        CancellationToken cancellationToken = default
    )
    {
        var specification = new UserRoleByUserAndRoleSpecification(userId: userId, roleId: roleId);
        return await context
            .UserRoles.ApplySpecification(specification: specification)
            .FirstOrDefaultAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<List<UserRoleEntity>> GetUserRolesWithRoleAsync(
        Guid userId,
        CancellationToken cancellationToken = default
    )
    {
        var specification = new UserRoleByUserIdSpecification(userId: userId);
        return await context
            .UserRoles.ApplySpecification(specification: specification)
            .Include(ur => ur.Role)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task AddAsync(UserRoleEntity entity, CancellationToken cancellationToken = default)
    {
        await context.UserRoles.AddAsync(entity, cancellationToken);
    }

    /// <inheritdoc />
    public void Delete(UserRoleEntity entity)
    {
        context.UserRoles.Remove(entity);
    }
}
