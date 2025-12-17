using _116.Core.Application.Shared.Repositories;
using _116.Core.Domain.Entities;
using _116.Identity.Application.Shared.Mappers;
using _116.Identity.Application.Shared.Repositories;
using _116.Identity.Domain.Entities;
using _116.Shared.Application.Exceptions;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Identity.Application.User.Public.UseCases.Queries.GetOwnProfile;

/// <summary>
/// Handles the <see cref="PublicGetOwnProfileQuery"/> to retrieve complete user profile information.
/// </summary>
/// <param name="userRepository">Repository for user data access operations.</param>
/// <param name="roleRepository">Repository for role and permission data operations.</param>
/// <param name="fileRepository">Repository for accessing file metadata.</param>
public class PublicGetOwnProfileHandler(
    IUserRepository userRepository,
    IRoleRepository roleRepository,
    IFileRepository fileRepository
) : IQueryHandler<PublicGetOwnProfileQuery, PublicGetOwnProfileResult>
{
    /// <summary>
    /// Handles the user profile query by retrieving complete user information with roles and permissions.
    /// </summary>
    /// <param name="query">The user profile query containing user ID.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A <see cref="PublicGetOwnProfileResult"/> containing complete user profile data.</returns>
    /// <exception cref="NotFoundException">Thrown when no user is found with the specified ID.</exception>
    /// <exception cref="BadRequestException">Thrown when the account is not active or verified.</exception>
    public async Task<PublicGetOwnProfileResult> Handle(
        PublicGetOwnProfileQuery query,
        CancellationToken cancellationToken
    )
    {
        UserEntity? user = await userRepository.GetUserWithRolesAndPermissionsByIdOrThrow(
            query.UserId,
            cancellationToken
        );
        // Validate user account status - must be active and verified
        userRepository.IsUserAccountActive(user!);
        userRepository.IsUserAccountVerified(user!);
        // Extract roles and permissions using repository
        var (roles, permissions) = roleRepository.GetUserRolesAndPermissions(user!.UserRoles);
        // Fetch the avatar file if the user has one
        FileEntity? avatarFile = await fileRepository.GetAvatarFileAsync(user.AvatarFileId, cancellationToken);
        // Map to userDTO with avatar
        var avatarDto = avatarFile?.ToFileDto();
        var userDto = user.ToUserResponseDto(roles, permissions, avatarDto);
        return new PublicGetOwnProfileResult(userDto);
    }
}
