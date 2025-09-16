using _116.Core.Application.Shared.Repositories;
using _116.Core.Domain.Entities;
using _116.Shared.Application.Exceptions;
using _116.Shared.Contracts.Application.CQRS;
using _116.User.Application.Shared.Mappers;
using _116.User.Application.Shared.Repositories;
using _116.User.Domain.Entities;

namespace _116.User.Application.Public.UseCases.Queries.GetUserProfile;

/// <summary>
/// Handles the <see cref="PublicGetUserProfileQuery"/> to retrieve complete user profile information.
/// </summary>
/// <param name="userRepository">Repository for user data access operations.</param>
/// <param name="roleRepository">Repository for role and permission data operations.</param>
/// <param name="fileRepository">Repository for accessing file metadata.</param>
public class PublicGetUserProfileHandler(
    IUserRepository userRepository,
    IRoleRepository roleRepository,
    IFileRepository fileRepository
) : IQueryHandler<PublicGetUserProfileQuery, PublicGetUserProfileResult>
{
    /// <summary>
    /// Handles the user profile query by retrieving complete user information with roles and permissions.
    /// </summary>
    /// <param name="query">The user profile query containing user ID.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A <see cref="PublicGetUserProfileResult"/> containing complete user profile data.</returns>
    /// <exception cref="NotFoundException">Thrown when no user is found with the specified ID.</exception>
    /// <exception cref="BadRequestException">Thrown when the account is not active or verified.</exception>
    public async Task<PublicGetUserProfileResult> Handle(
        PublicGetUserProfileQuery query,
        CancellationToken cancellationToken
    )
    {
        // Get user with roles by ID
        UserEntity? user = await userRepository.GetUserWithRolesAndPermissionsByIdAsync(query.UserId, cancellationToken);
        // Validate user account status - must be active and verified
        userRepository.IsUserAccountActive(user!);
        userRepository.IsUserAccountVerified(user!);

        // Extract roles and permissions using repository
        var (roles, permissions) = roleRepository.GetUserRolesAndPermissions(user.UserRoles);

        // Fetch the avatar file if the user has one
        FileEntity? avatarFile = user.AvatarFileId.HasValue
            ? await fileRepository.GetByIdAsync(user.AvatarFileId.Value, cancellationToken)
            : null;

        // Map to userDTO with avatar
        var avatarDto = avatarFile.ToFileDto();
        var userDto = user.ToUserResponseDto(roles, permissions, avatarDto);

        return new PublicGetUserProfileResult(userDto);
    }
}
