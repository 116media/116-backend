using _116.BuildingBlocks.Application.CQRS;
using _116.Identity.Application.Roles.UseCases.Admin.Commands.SoftDeleteRole.Contracts;
using _116.Identity.Application.Shared.Mappers;
using _116.Identity.Application.Shared.Persistence;
using _116.Identity.Domain.Entities;
using MapsterMapper;

namespace _116.Identity.Application.Roles.UseCases.Admin.Commands.SoftDeleteRole;

/// <summary>
/// Handles the <see cref="AdminSoftDeleteRoleCommand" /> to soft-delete a role.
/// </summary>
/// <param name="softDeleteRoleService">Service loading the role and applying the deletion.</param>
/// <param name="unitOfWork">Unit of Work for managing database transactions.</param>
/// <param name="mapper">Mapster mapper for entity-to-DTO transformations.</param>
public class AdminSoftDeleteRoleHandler(
    IAdminSoftDeleteRoleService softDeleteRoleService,
    IIdentityUnitOfWork unitOfWork,
    IMapper mapper
) : ICommandHandler<AdminSoftDeleteRoleCommand, AdminSoftDeleteRoleResult>
{
    /// <inheritdoc />
    public async Task<AdminSoftDeleteRoleResult> Handle(
        AdminSoftDeleteRoleCommand command,
        CancellationToken cancellationToken
    )
    {
        Guid roleId = Guid.Parse(input: command.RoleId);

        RoleEntity role = await softDeleteRoleService.SoftDeleteAsync(
            roleId: roleId,
            cancellationToken: cancellationToken
        );

        await unitOfWork.CommitAsync(cancellationToken: cancellationToken);

        var roleDto = role.ToRoleDto(mapper);
        return new AdminSoftDeleteRoleResult(Role: roleDto, IsSuccess: true);
    }
}
