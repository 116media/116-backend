using _116.Core.Application.Shared.Repositories;
using _116.Core.Domain.Entities;
using _116.Identity.Application.Shared.Mappers;
using _116.Identity.Application.Shared.Repositories;
using _116.Identity.Domain.Entities;
using _116.Shared.Application.Exceptions;
using _116.Shared.Contracts.Application.CQRS;
using MapsterMapper;

namespace _116.Identity.Application.User.UseCases.Public.Queries.GetOwnProfile;

/// <summary>
/// Handles the <see cref="PublicGetOwnProfileQuery" /> to retrieve complete user profile information.
/// </summary>
/// <param name="authRepository">Repository for user data access operations.</param>
/// <param name="roleRepository">Repository for role and permission data operations.</param>
/// <param name="fileRepository">Repository for accessing file metadata.</param>
/// <param name="mapper">Mapster mapper for entity-to-DTO transformations.</param>
public class PublicGetOwnProfileHandler(
    IAuthRepository authRepository,
    IRoleRepository roleRepository,
    IFileRepository fileRepository,
    IMapper mapper
) : IQueryHandler<PublicGetOwnProfileQuery, PublicGetOwnProfileResult>
{
    /// <summary>
    /// Handles the user profile query by retrieving complete user information with roles and permissions.
    /// </summary>
    /// <param name="query">The user profile query containing user ID.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A <see cref="PublicGetOwnProfileResult" /> containing complete user profile data.</returns>
    /// <exception cref="NotFoundException">Thrown when no user is found with the specified ID.</exception>
    /// <exception cref="BadRequestException">Thrown when the account is not active or verified.</exception>
    public async Task<PublicGetOwnProfileResult> Handle(
        PublicGetOwnProfileQuery query,
        CancellationToken cancellationToken
    )
    {
        UserEntity? user = await authRepository.GetUserWithRolesAndPermissionsByIdOrThrow(
            userId: query.UserId,
            cancellationToken: cancellationToken
        );

        authRepository.IsUserAccountActive(user!);
        authRepository.IsUserAccountVerified(user!);

        var (roles, permissions) = roleRepository.GetUserRolesAndPermissions(userRoles: user!.UserRoles);

        // Fetch the avatar file if the user has one
        FileEntity? avatarFile = await fileRepository.GetAvatarFileAsync(
            avatarFileId: user.AvatarFileId,
            cancellationToken: cancellationToken
        );

        var avatarDto = avatarFile?.ToFileDto(mapper);
        var userDto = user.ToUserResponseDto(mapper: mapper, roles: roles, permissions: permissions, avatar: avatarDto);

        return new PublicGetOwnProfileResult(User: userDto);
    }
}
