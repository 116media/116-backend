using _116.Identity.Application.Roles.Builders;
using _116.Identity.Application.Roles.Specifications;
using _116.Identity.Application.Shared.DTOs;
using _116.Identity.Application.Shared.Repositories;
using _116.Identity.Domain.Entities;
using _116.Identity.Infrastructure.Persistence;
using _116.Shared.Application.Specifications;
using _116.Shared.Infrastructure.Extensions;
using Mapster;
using Microsoft.EntityFrameworkCore;

namespace _116.Identity.Infrastructure.Repositories;

/// <summary>
/// Implementation of <see cref="IRoleRepository" /> for processing user roles and permissions.
/// </summary>
/// <param name="context">The database context for accessing role data.</param>
public class RoleRepository(IdentityDbContext context) : IRoleRepository
{
    /// <inheritdoc />
    public async Task<RoleEntity?> GetRoleByIdWithPermissionsOrThrowAsync(
        Guid roleId,
        CancellationToken cancellationToken = default
    )
    {
        var specification = new RoleByIdSpecification(roleId: roleId);
        return await context
            .Roles.ApplySpecification(specification: specification)
            .Include(r => r.RolePermissions)
                .ThenInclude(rp => rp.Permission)
            .AsSplitQuery()
            .FirstDefaultOrThrowAsync(keyValue: roleId, cancellationToken: cancellationToken);
    }

    /// <inheritdoc />
    public async Task<RoleEntity?> GetRoleByIdOrThrowAsync(Guid roleId, CancellationToken cancellationToken = default)
    {
        var specification = new RoleByIdSpecification(roleId: roleId);
        return await context
            .Roles.ApplySpecification(specification: specification)
            .FirstDefaultOrThrowAsync(keyValue: roleId, cancellationToken: cancellationToken);
    }

    /// <inheritdoc />
    public async Task<bool> ExistsByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        var specification = new RoleByNameSpecification(roleName: name);
        return await context.Roles.ApplySpecification(specification: specification).AnyAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task AddAsync(RoleEntity role, CancellationToken cancellationToken = default)
    {
        await context.Roles.AddAsync(role, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<(List<RoleEntity> Roles, int TotalCount)> GetAllWithPaginationAsync(
        int page,
        int pageSize,
        string? search = null,
        bool? isActive = null,
        bool? isDeleted = null,
        CancellationToken cancellationToken = default
    )
    {
        IRoleQueryBuilder builder = new RoleQueryBuilder()
            .WithSearch(search: search)
            .WithActiveStatus(isActive: isActive)
            .WithDeletedStatus(isDeleted: isDeleted);

        Specification<RoleEntity>? spec = builder.Build();

        IQueryable<RoleEntity> query = spec is not null ? context.Roles.Where(spec.ToExpression()) : context.Roles;

        int totalCount = await query.CountAsync(cancellationToken: cancellationToken);

        List<RoleEntity> roles = await query
            .OrderByDescending(r => r.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken: cancellationToken);

        return (roles, totalCount);
    }

    /// <inheritdoc />
    public IReadOnlyCollection<RoleDto> GetUserRoles(ICollection<UserRoleEntity> userRoles)
    {
        return userRoles.Select(ur => ur.Role.Adapt<RoleDto>()).ToList();
    }

    /// <inheritdoc />
    public IReadOnlyCollection<PermissionDto> GetUserPermissions(ICollection<UserRoleEntity> userRoles)
    {
        return userRoles
            .SelectMany(ur => ur.Role.RolePermissions)
            .Select(rp => rp.Permission.Adapt<PermissionDto>())
            .DistinctBy(p => p.Id)
            .ToList();
    }

    /// <inheritdoc />
    public (
        IReadOnlyCollection<RoleDto> Roles,
        IReadOnlyCollection<PermissionDto> Permissions
    ) GetUserRolesAndPermissions(ICollection<UserRoleEntity> userRoles)
    {
        IReadOnlyCollection<RoleDto> roles = GetUserRoles(userRoles: userRoles);
        IReadOnlyCollection<PermissionDto> permissions = GetUserPermissions(userRoles: userRoles);
        return (roles, permissions);
    }

    /// <inheritdoc />
    public void Delete(RoleEntity entity)
    {
        context.Roles.Remove(entity);
    }
}
