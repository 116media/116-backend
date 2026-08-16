using _116.Identity.Application.Session.Repositories;
using _116.Identity.Application.Shared.Errors;
using _116.Identity.Application.Shared.Persistence;
using _116.Identity.Application.Shared.Repositories;
using _116.Identity.Application.User.UseCases.Public.Commands.UpdateOwnProfile.Contracts;
using _116.Identity.Domain.Entities;
using _116.Identity.Domain.Enums;
using _116.Identity.Domain.ValueObjects;

namespace _116.Identity.Application.User.UseCases.Public.Commands.UpdateOwnProfile;

/// <summary>
/// Factory implementation for handling user profile update logic. An email change revokes the
/// account's other sessions — keeping the acting session alive — and rotates the security stamp.
/// </summary>
/// <param name="authRepository">Repository for user data access operations.</param>
/// <param name="sessionRepository">Repository revoking the user's sessions.</param>
/// <param name="tokenStateRepository">Repository rotating the user's security stamp.</param>
/// <param name="unitOfWork">Unit of Work for managing database transactions.</param>
/// <param name="userErrors">User domain error factory for generating domain exceptions.</param>
public class PublicUpdateProfileAuthFactory(
    IAuthRepository authRepository,
    ISessionRepository sessionRepository,
    IUserTokenStateRepository tokenStateRepository,
    IIdentityUnitOfWork unitOfWork,
    UserErrors userErrors
) : IPublicUpdateProfileAuthFactory
{
    /// <summary>
    /// Updates a user's profile with new information.
    /// </summary>
    public async Task<PublicUpdateProfileAuthData> UpdateProfileAsync(
        Guid userId,
        Guid sessionId,
        string? email,
        string? userName,
        string? countryName,
        string? countryIsoCode,
        string? countryDialCode,
        string? partialPhoneNumber,
        CancellationToken cancellationToken
    )
    {
        UserEntity? user = await authRepository.GetUserWithRolesAndPermissionsByIdOrThrow(
            userId: userId,
            cancellationToken: cancellationToken
        );

        authRepository.IsUserAccountActive(user!);
        authRepository.IsUserAccountVerified(user!);
        await authRepository.IsSessionValidAsync(sessionId, cancellationToken);

        bool isPhoneUpdated = !string.IsNullOrEmpty(value: partialPhoneNumber);
        bool isUsernameUpdated = !string.IsNullOrEmpty(value: userName) && user!.UserName != userName;
        bool isEmailUpdated = !string.IsNullOrEmpty(value: email) && user!.Email != email?.ToLowerInvariant();

        if (isEmailUpdated)
        {
            await EnsureEmailUnique(email!, cancellationToken: cancellationToken);
            user!.UpdateEmail(newEmail: email!, errors: userErrors);

            // The acting session survives the change it performed; the account's other sessions
            // are revoked in the same transaction as the new address.
            await sessionRepository.DeleteAllByUserIdAsync(
                userId: user.Id,
                reason: EnumSessionRevokeReason.SecurityInvalidation,
                exemptSessionId: sessionId,
                cancellationToken: cancellationToken
            );
        }

        if (isUsernameUpdated)
        {
            await EnsureUsernameUnique(userName!, cancellationToken: cancellationToken);
            user!.UpdateUserName(newUserName: userName!, errors: userErrors);
        }

        if (isPhoneUpdated)
        {
            await EnsurePhoneUnique(
                userId: userId,
                countryDialCode: countryDialCode!,
                partialPhoneNumber: partialPhoneNumber!,
                cancellationToken: cancellationToken
            );
            user!.UpdatePhoneNumber(
                countryName: countryName,
                countryIsoCode: countryIsoCode,
                countryDialCode: countryDialCode,
                partialPhoneNumber: partialPhoneNumber,
                fullPhoneNumber: $"{countryDialCode}{partialPhoneNumber}"
            );
        }

        await unitOfWork.CommitAsync(cancellationToken: cancellationToken);

        if (isEmailUpdated)
        {
            await tokenStateRepository.RotateSecurityStampAsync(userId: user!.Id, cancellationToken: cancellationToken);
        }

        return new PublicUpdateProfileAuthData(User: user!);
    }

    private async Task EnsureEmailUnique(string email, CancellationToken cancellationToken)
    {
        if (await authRepository.ExistsByEmailAsync(new Email(value: email), cancellationToken: cancellationToken))
        {
            throw userErrors.EmailAlreadyExists(email: email);
        }
    }

    private async Task EnsureUsernameUnique(string username, CancellationToken cancellationToken)
    {
        if (await authRepository.ExistsByUserNameAsync(userName: username, cancellationToken: cancellationToken))
        {
            throw userErrors.UsernameAlreadyExists(username: username);
        }
    }

    private async Task EnsurePhoneUnique(
        Guid userId,
        string countryDialCode,
        string partialPhoneNumber,
        CancellationToken cancellationToken
    )
    {
        string fullPhone = $"{countryDialCode}{partialPhoneNumber}";
        UserEntity? existing = await authRepository.GetUserByPhoneNumberAsync(
            phoneNumber: fullPhone,
            cancellationToken: cancellationToken
        );

        if (existing is not null && existing.Id != userId)
        {
            throw userErrors.PhoneNumberAlreadyExists(phoneNumber: fullPhone);
        }
    }
}
