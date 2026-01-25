using _116.Identity.Application.Roles.Builders;
using _116.Identity.Application.Roles.Specifications;
using _116.Identity.Application.Shared.Repositories;
using _116.Identity.Domain.Entities;
using _116.Identity.Infrastructure.Persistence;
using _116.Shared.Application.Specifications;
using _116.Shared.Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;

namespace _116.Identity.Infrastructure.Repositories;

/// <summary>
/// Implementation of <see cref="IPermissionRepository" /> for managing permission entities.
/// </summary>
/// <param name="context">The database context for accessing permission data.</param>
public class PermissionRepository(IdentityDbContext context) : IPermissionRepository
{
    /// <inheritdoc />
    public async Task<PermissionEntity?> GetPermissionByIdOrThrowAsync(
        Guid permissionId,
        CancellationToken cancellationToken = default
    )
    {
        var specification = new PermissionByIdSpecification(permissionId: permissionId);
        return await context
            .Permissions.ApplySpecification(specification: specification)
            .FirstDefaultOrThrowAsync(keyValue: permissionId, cancellationToken: cancellationToken);
    }

    /// <inheritdoc />
    public async Task<bool> ExistsByResourceAndActionAsync(
        string resource,
        string action,
        CancellationToken cancellationToken = default
    )
    {
        var specification = new PermissionByResourceAndActionSpecification(resource: resource, action: action);
        return await context.Permissions.ApplySpecification(specification: specification).AnyAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task AddAsync(PermissionEntity permission, CancellationToken cancellationToken = default)
    {
        await context.Permissions.AddAsync(permission, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<(List<PermissionEntity> Permissions, int TotalCount)> GetAllWithPaginationAsync(
        int page,
        int pageSize,
        string? search = null,
        bool? isActive = null,
        bool? isDeleted = null,
        CancellationToken cancellationToken = default
    )
    {
        IPermissionQueryBuilder builder = new PermissionQueryBuilder()
            .WithSearch(search: search)
            .WithActiveStatus(isActive: isActive)
            .WithDeletedStatus(isDeleted: isDeleted);

        Specification<PermissionEntity>? spec = builder.Build();

        IQueryable<PermissionEntity> query = spec is not null
            ? context.Permissions.Where(spec.ToExpression())
            : context.Permissions;

        int totalCount = await query.CountAsync(cancellationToken: cancellationToken);

        List<PermissionEntity> permissions = await query
            .OrderByDescending(p => p.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken: cancellationToken);

        return (permissions, totalCount);
    }

    /// <inheritdoc />
    public void Delete(PermissionEntity entity)
    {
        context.Permissions.Remove(entity);
    }
}
