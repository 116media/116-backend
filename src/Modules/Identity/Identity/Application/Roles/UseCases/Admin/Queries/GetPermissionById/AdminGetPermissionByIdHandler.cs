using _116.Identity.Application.Shared.DTOs;
using _116.Identity.Application.Shared.Mappers;
using _116.Identity.Application.Shared.Repositories;
using _116.Identity.Domain.Entities;
using _116.Shared.Contracts.Application.CQRS;
using MapsterMapper;

namespace _116.Identity.Application.Roles.UseCases.Admin.Queries.GetPermissionById;

/// <summary>
/// Handles the <see cref="AdminGetPermissionByIdQuery" /> to retrieve a permission by its ID.
/// </summary>
/// <param name="permissionRepository">Repository for permission data access operations.</param>
/// <param name="mapper">Mapster mapper for entity-to-DTO transformations.</param>
public class AdminGetPermissionByIdHandler(IPermissionRepository permissionRepository, IMapper mapper)
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
        PermissionEntity? permission = await permissionRepository.GetPermissionByIdOrThrowAsync(
            permissionId: query.PermissionId,
            cancellationToken: cancellationToken
        );

        var permissionDto = permission!.ToPermissionDto(mapper);

        return new AdminGetPermissionByIdResult(Permission: permissionDto);
    }
}
