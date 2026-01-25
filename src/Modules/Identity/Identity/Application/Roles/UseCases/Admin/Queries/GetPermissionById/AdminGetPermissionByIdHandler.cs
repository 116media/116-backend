using _116.Identity.Application.Shared.DTOs;
using _116.Identity.Application.Shared.Mappers;
using _116.Identity.Application.Shared.Repositories;
using _116.Identity.Domain.Entities;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Identity.Application.Roles.UseCases.Admin.Queries.GetPermissionById;

/// <summary>
/// Handles the <see cref="AdminGetPermissionByIdQuery" /> to retrieve a permission by its ID.
/// </summary>
/// <param name="permissionRepository">Repository for permission data access operations.</param>
public class AdminGetPermissionByIdHandler(IPermissionRepository permissionRepository)
    : IQueryHandler<AdminGetPermissionByIdQuery, AdminGetPermissionByIdResult>
{
    /// <summary>
    /// Handles the query by fetching the permission.
    /// </summary>
    /// <param name="query">The query containing the permission ID.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A <see cref="AdminGetPermissionByIdResult" /> containing the permission.</returns>
    public async Task<AdminGetPermissionByIdResult> Handle(
        AdminGetPermissionByIdQuery query,
        CancellationToken cancellationToken
    )
    {
        Guid permissionId = Guid.Parse(input: query.PermissionId);

        PermissionEntity? permission = await permissionRepository.GetPermissionByIdOrThrowAsync(
            permissionId: permissionId,
            cancellationToken: cancellationToken
        );

        var permissionDto = permission!.ToPermissionDto();

        return new AdminGetPermissionByIdResult(Permission: permissionDto);
    }
}
