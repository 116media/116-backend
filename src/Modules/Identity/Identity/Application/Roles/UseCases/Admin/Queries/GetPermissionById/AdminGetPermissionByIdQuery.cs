using _116.Identity.Application.Shared.Cache;
using _116.Identity.Application.Shared.DTOs;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Identity.Application.Roles.UseCases.Admin.Queries.GetPermissionById;

/// <summary>
/// Query to retrieve a permission by its unique identifier.
/// </summary>
/// <param name="PermissionId">The unique identifier of the permission to retrieve.</param>
public record AdminGetPermissionByIdQuery(Guid PermissionId) : IQuery<AdminGetPermissionByIdResult>, ICacheableRequest
{
    /// <inheritdoc />
    public string CacheKey => $"lookup:permissions:{PermissionId}";

    /// <inheritdoc />
    public TimeSpan Ttl => TimeSpan.FromMinutes(30);

    /// <inheritdoc />
    public IReadOnlyList<string> CacheTags => [IdentityCacheTags.Lookups];
}

/// <summary>
/// The result of executing an <see cref="AdminGetPermissionByIdQuery" />.
/// </summary>
/// <param name="Permission">The permission details.</param>
public record AdminGetPermissionByIdResult(PermissionDto Permission);
