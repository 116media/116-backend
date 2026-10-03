using _116.BuildingBlocks.Application.CQRS;
using _116.Identity.Application.Shared.DTOs;
using _116.Identity.Application.Shared.Mappers;
using _116.Identity.Application.Shared.Persistence;
using _116.Identity.Application.Shared.Repositories;
using _116.Identity.Application.User.UseCases.Admin.Commands.AssignRoleToUser.Contracts;
using MapsterMapper;

namespace _116.Identity.Application.User.UseCases.Admin.Commands.AssignRoleToUser;

/// <summary>
/// Handles the <see cref="AdminAssignRoleToUserCommand" /> to grant a role through the user
/// aggregate, bumping the target user's token version so outstanding tokens pick up the grant
/// on refresh.
/// </summary>
/// <param name="assignRoleService">Service resolving and applying the grant.</param>
/// <param name="tokenStateRepository">Repository bumping the target user's token version.</param>
/// <param name="unitOfWork">Unit of Work for managing database transactions.</param>
/// <param name="mapper">Mapster mapper for entity-to-DTO transformations.</param>
public class AdminAssignRoleToUserHandler(
    IAdminAssignRoleToUserService assignRoleService,
    IUserTokenStateRepository tokenStateRepository,
    IIdentityUnitOfWork unitOfWork,
    IMapper mapper
) : ICommandHandler<AdminAssignRoleToUserCommand, AdminAssignRoleToUserResult>
{
    /// <inheritdoc />
    public async Task<AdminAssignRoleToUserResult> Handle(
        AdminAssignRoleToUserCommand command,
        CancellationToken cancellationToken
    )
    {
        Guid userId = Guid.Parse(input: command.UserId);

        RoleGrantData grant = await assignRoleService.GrantAsync(
            userId: userId,
            roleId: command.RoleId,
            cancellationToken: cancellationToken
        );

        await unitOfWork.CommitAsync(cancellationToken: cancellationToken);
        await tokenStateRepository.BumpTokenVersionAsync(userId: userId, cancellationToken: cancellationToken);

        // The freshly granted association has no Role navigation loaded yet; the role in hand fills it.
        IReadOnlyCollection<RoleDto> roles = grant
            .User.UserRoles.Select(ur => (ur.RoleId == grant.Role.Id ? grant.Role : ur.Role).ToRoleDto(mapper))
            .ToList();
        return new AdminAssignRoleToUserResult(Roles: roles);
    }
}
