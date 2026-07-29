using _116.Identity.Application.Shared.Errors;
using _116.Identity.Application.Shared.Persistence;
using _116.Identity.Application.Shared.Repositories;
using _116.Identity.Application.User.UseCases.Public.Commands.UpdateOwnProfile.Contracts;
using _116.Identity.Domain.Entities;
using _116.Identity.Domain.ValueObjects;
using _116.Mailer.Contracts.Application;

namespace _116.Identity.Application.User.UseCases.Public.Commands.UpdateOwnProfile;

/// <summary>
/// Factory implementation for handling user profile update logic.
/// </summary>
/// <param name="authRepository">Repository for user data access operations.</param>
/// <param name="unitOfWork">Unit of Work for managing database transactions.</param>
/// <param name="userErrors">User domain error factory for generating domain exceptions.</param>
public class PublicUpdateProfileAuthFactory(
    IAuthRepository authRepository,
    IIdentityUnitOfWork unitOfWork,
    UserErrors userErrors,
    IMailer mailer
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

        // Captured before the aggregate mutates: the alert must reach the
        // address that is about to lose the account.
        string? previousEmail = user!.Email;

        if (isEmailUpdated)
        {
            await EnsureEmailUnique(email!, cancellationToken: cancellationToken);
            user.UpdateEmail(newEmail: email!, errors: userErrors);
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
            await NotifyEmailChangedAsync(user!, previousEmail, cancellationToken);
        }

        return new PublicUpdateProfileAuthData(User: user!);
    }

    /// <summary>
    /// Notifies both sides of an email change: an alert to the old address
    /// (which just lost the account) and a confirmation to the new one. The
    /// new address is masked in the alert so a compromised old mailbox never
    /// learns it in full.
    /// </summary>
    private async Task NotifyEmailChangedAsync(
        UserEntity user,
        string? previousEmail,
        CancellationToken cancellationToken
    )
    {
        string changeTime = DateTime.UtcNow.ToString("u");

        if (previousEmail is not null)
        {
            await mailer.EnqueueAsync(
                template: EnumEmailTemplate.EmailChangedAlertOld,
                to: new EmailRecipient(Address: previousEmail, DisplayName: user.UserName),
                tokens: new Dictionary<string, string>
                {
                    ["userName"] = user.UserName,
                    ["newEmailMasked"] = MaskEmail(user.Email!),
                    ["changeTime"] = changeTime,
                },
                culture: EmailCulture.Current(),
                cancellationToken: cancellationToken
            );
        }

        await mailer.EnqueueAsync(
            template: EnumEmailTemplate.EmailChangedConfirmNew,
            to: new EmailRecipient(Address: user.Email!, DisplayName: user.UserName),
            tokens: new Dictionary<string, string> { ["userName"] = user.UserName, ["changeTime"] = changeTime },
            culture: EmailCulture.Current(),
            cancellationToken: cancellationToken
        );
    }

    /// <summary>
    /// Masks the local part of an address (j***@example.com), keeping only its
    /// first character.
    /// </summary>
    private static string MaskEmail(string email)
    {
        int at = email.IndexOf('@');

        if (at <= 1)
        {
            return $"***{email[at..]}";
        }

        return $"{email[0]}***{email[at..]}";
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
