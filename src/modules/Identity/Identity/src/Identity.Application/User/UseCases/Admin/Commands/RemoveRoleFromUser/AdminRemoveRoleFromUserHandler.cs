using _116.BuildingBlocks.Application.CQRS;
using _116.Identity.Application.Shared.DTOs;
using _116.Identity.Application.Shared.Mappers;
using _116.Identity.Application.Shared.Persistence;
using _116.Identity.Application.Shared.Repositories;
using _116.Identity.Application.User.UseCases.Admin.Commands.RemoveRoleFromUser.Contracts;
using _116.Identity.Domain.Entities;
using MapsterMapper;

namespace _116.Identity.Application.User.UseCases.Admin.Commands.RemoveRoleFromUser;

/// <summary>
/// Handles the <see cref="AdminRemoveRoleFromUserCommand" /> to revoke a role through the user
/// aggregate, bumping the target user's token version so the removed role cannot keep riding a
/// live token.
/// </summary>
/// <param name="removeRoleService">Service resolving and applying the revocation.</param>
/// <param name="tokenStateRepository">Repository bumping the target user's token version.</param>
/// <param name="unitOfWork">Unit of Work for managing database transactions.</param>
/// <param name="mapper">Mapster mapper for entity-to-DTO transformations.</param>
public class AdminRemoveRoleFromUserHandler(
    IAdminRemoveRoleFromUserService removeRoleService,
    IUserTokenStateRepository tokenStateRepository,
    IIdentityUnitOfWork unitOfWork,
    IMapper mapper
) : ICommandHandler<AdminRemoveRoleFromUserCommand, AdminRemoveRoleFromUserResult>
{
    /// <inheritdoc />
    public async Task<AdminRemoveRoleFromUserResult> Handle(
        AdminRemoveRoleFromUserCommand command,
        CancellationToken cancellationToken
    )
    {
        Guid userId = Guid.Parse(input: command.UserId);
        Guid roleId = Guid.Parse(input: command.RoleId);

        UserEntity user = await removeRoleService.RevokeAsync(
            userId: userId,
            roleId: roleId,
            cancellationToken: cancellationToken
        );

        await unitOfWork.CommitAsync(cancellationToken: cancellationToken);
        await tokenStateRepository.BumpTokenVersionAsync(userId: userId, cancellationToken: cancellationToken);

        IReadOnlyCollection<RoleDto> roles = user.UserRoles.ToRoleDtos(mapper);
        return new AdminRemoveRoleFromUserResult(Roles: roles, IsSuccess: true);
    }
}
