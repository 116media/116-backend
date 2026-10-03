using _116.BuildingBlocks.Application.CQRS;
using _116.BuildingBlocks.Application.Exceptions;
using _116.Identity.Application.Shared.Mappers;
using _116.Identity.Application.User.Ports;
using _116.Identity.Application.User.UseCases.Admin.Commands.UpdateOwnProfile.Contracts;
using _116.Storage.Contracts.Application.DTOs;
using MapsterMapper;

namespace _116.Identity.Application.User.UseCases.Admin.Commands.UpdateOwnProfile;

/// <summary>
/// Handles the <see cref="AdminUpdateOwnProfileCommand" /> to update admin user's own profile information.
/// This endpoint requires admin user authentication - only logged-in admin users can update their own profile.
/// </summary>
/// <param name="authService">Service for handling admin user profile update logic.</param>
/// <param name="avatarService">Resolves and stores the user's avatar.</param>
/// <param name="mapper">Mapster mapper for entity-to-DTO transformations.</param>
public class AdminUpdateOwnProfileHandler(
    IAdminUpdateProfileAuthService authService,
    IAvatarService avatarService,
    IMapper mapper
) : ICommandHandler<AdminUpdateOwnProfileCommand, AdminUpdateOwnProfileResult>
{
    /// <summary>
    /// Handles the profile update command by validating uniqueness and updating admin user information.
    /// </summary>
    /// <param name="command">The profile update command containing user ID and new profile data.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A <see cref="AdminUpdateOwnProfileResult" /> containing updated admin user profile data.</returns>
    /// <exception cref="NotFoundException">Thrown when no user is found with the specified ID.</exception>
    /// <exception cref="BadRequestException">Thrown when the account is not active.</exception>
    /// <exception cref="ConflictException">Thrown when username or phone number already exists.</exception>
    public async Task<AdminUpdateOwnProfileResult> Handle(
        AdminUpdateOwnProfileCommand command,
        CancellationToken cancellationToken
    )
    {
        AdminUpdateProfileAuthData authData = await authService.UpdateProfileAsync(
            userId: command.UserId,
            sessionId: command.SessionId,
            userName: command.UserName,
            countryName: command.CountryName,
            countryIsoCode: command.CountryIsoCode,
            countryDialCode: command.CountryDialCode,
            partialPhoneNumber: command.PartialPhoneNumber,
            preferredLocale: command.PreferredLocale,
            cancellationToken: cancellationToken
        );

        FileDto? avatarDto = await avatarService.GetAvatarAsync(
            avatarFileId: authData.User.AvatarFileId,
            cancellationToken: cancellationToken
        );
        var userDto = authData.User.ToUserResponseDto(
            mapper: mapper,
            roles: authData.User.UserRoles.ToRoleDtos(mapper),
            permissions: authData.User.UserRoles.ToPermissionDtos(mapper),
            avatar: avatarDto
        );
        return new AdminUpdateOwnProfileResult(User: userDto);
    }
}
