using _116.Identity.Application.Shared.Errors;
using _116.Identity.Application.Shared.Persistence;
using _116.Identity.Application.Shared.Repositories;
using _116.Identity.Application.User.UseCases.Admin.Commands.UpdateOwnProfile.Contracts;
using _116.Identity.Domain.Entities;

namespace _116.Identity.Application.User.UseCases.Admin.Commands.UpdateOwnProfile;

/// <summary>
/// Factory implementation for handling admin user profile update logic.
/// </summary>
/// <param name="authRepository">Repository for user data access operations.</param>
/// <param name="unitOfWork">Unit of Work for managing database transactions.</param>
/// <param name="userErrors">User domain error factory for generating domain exceptions.</param>
public class AdminUpdateProfileAuthFactory(
    IAuthRepository authRepository,
    IIdentityUnitOfWork unitOfWork,
    UserErrors userErrors
) : IAdminUpdateProfileAuthFactory
{
    /// <summary>
    /// Updates an admin user's profile with new information.
    /// </summary>
    public async Task<AdminUpdateProfileAuthData> UpdateProfileAsync(
        Guid userId,
        Guid sessionId,
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
        await authRepository.IsSessionValidAsync(sessionId, cancellationToken);

        // Blank means "not supplied", matching the optional validators, which skip their rules
        // when the value is whitespace.
        bool isPhoneUpdated = !string.IsNullOrWhiteSpace(value: partialPhoneNumber);
        bool isUsernameUpdated = !string.IsNullOrWhiteSpace(value: userName) && user!.UserName != userName;

        if (isUsernameUpdated)
        {
            await EnsureUsernameUnique(userName!, cancellationToken: cancellationToken);
            user!.UpdateUserName(newUserName: userName!);
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

        return new AdminUpdateProfileAuthData(User: user!);
    }

    /// <summary>
    /// Refuses a username already registered to an account.
    /// </summary>
    /// <param name="username">The requested username.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <exception cref="_116.Shared.Application.Exceptions.ConflictException">
    /// Thrown when the username is taken.
    /// </exception>
    private async Task EnsureUsernameUnique(string username, CancellationToken cancellationToken)
    {
        if (await authRepository.ExistsByUserNameAsync(userName: username, cancellationToken: cancellationToken))
        {
            throw userErrors.UsernameAlreadyExists(username: username);
        }
    }

    /// <summary>
    /// Refuses a phone number held by a different account; the caller keeping its own number is
    /// not a conflict.
    /// </summary>
    /// <param name="userId">The account being updated.</param>
    /// <param name="countryDialCode">The dial code the number is prefixed with.</param>
    /// <param name="partialPhoneNumber">The local part of the number.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <exception cref="_116.Shared.Application.Exceptions.ConflictException">
    /// Thrown when another account holds the number.
    /// </exception>
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
