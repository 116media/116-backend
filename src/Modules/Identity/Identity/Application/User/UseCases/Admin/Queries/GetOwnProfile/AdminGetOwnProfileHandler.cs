using _116.Core.Application.Shared.Repositories;
using _116.Core.Domain.Entities;
using _116.Identity.Application.Shared.Mappers;
using _116.Identity.Application.Shared.Repositories;
using _116.Identity.Domain.Entities;
using _116.Shared.Application.Exceptions;
using _116.Shared.Contracts.Application.CQRS;
using MapsterMapper;

namespace _116.Identity.Application.User.UseCases.Admin.Queries.GetOwnProfile;

/// <summary>
/// Handles the <see cref="AdminGetOwnProfileQuery" /> to retrieve complete admin user profile information.
/// </summary>
/// <param name="authRepository">Repository for user data access operations.</param>
/// <param name="roleRepository">Repository for role and permission data operations.</param>
/// <param name="fileRepository">Repository for accessing file metadata.</param>
/// <param name="mapper">Mapster mapper for entity-to-DTO transformations.</param>
public class AdminGetOwnProfileHandler(IAuthRepository authRepository, IFileRepository fileRepository, IMapper mapper)
    : IQueryHandler<AdminGetOwnProfileQuery, AdminGetOwnProfileResult>
{
    /// <summary>
    /// Handles the admin user profile query by retrieving complete user information with roles and permissions.
    /// </summary>
    /// <param name="query">The admin user profile query containing user ID.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A <see cref="AdminGetOwnProfileResult" /> containing complete admin user profile data.</returns>
    /// <exception cref="NotFoundException">Thrown when no user is found with the specified ID.</exception>
    /// <exception cref="BadRequestException">Thrown when the account is not active.</exception>
    public async Task<AdminGetOwnProfileResult> Handle(
        AdminGetOwnProfileQuery query,
        CancellationToken cancellationToken
    )
    {
        UserEntity? user = await authRepository.GetUserWithRolesAndPermissionsByIdOrThrow(
            userId: query.UserId,
            cancellationToken: cancellationToken
        );
        // Validate user account status - admin accounts must be active
        authRepository.IsUserAccountActive(user!);
        // Extract roles and permissions from loaded user roles
        var roles = user!.UserRoles.ToRoleDtos(mapper);
        var permissions = user.UserRoles.ToPermissionDtos(mapper);
        // Fetch the avatar file if the user has one
        FileEntity? avatarFile = await fileRepository.GetAvatarFileAsync(
            avatarFileId: user.AvatarFileId,
            cancellationToken: cancellationToken
        );

        var avatarDto = avatarFile?.ToFileDto(mapper);
        var userDto = user.ToUserResponseDto(mapper: mapper, roles: roles, permissions: permissions, avatar: avatarDto);
        return new AdminGetOwnProfileResult(User: userDto);
    }
}
