using _116.Identity.Application.Shared.DTOs;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Identity.Application.Roles.UseCases.Admin.Queries.GetPermissionById;

/// <summary>
/// Query to retrieve a permission by its unique identifier.
/// </summary>
/// <param name="PermissionId">The unique identifier of the permission to retrieve.</param>
public record AdminGetPermissionByIdQuery(string PermissionId) : IQuery<AdminGetPermissionByIdResult>;

/// <summary>
/// The result of executing an <see cref="AdminGetPermissionByIdQuery" />.
/// </summary>
/// <param name="Permission">The permission details.</param>
public record AdminGetPermissionByIdResult(PermissionDto Permission);
