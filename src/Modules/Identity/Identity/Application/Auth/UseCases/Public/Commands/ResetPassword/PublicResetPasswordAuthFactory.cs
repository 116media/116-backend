using _116.Identity.Application.Auth.Services;
using _116.Identity.Application.Auth.UseCases.Public.Commands.ResetPassword.Contracts;
using _116.Identity.Application.Session.Repositories;
using _116.Identity.Application.Shared.Errors;
using _116.Identity.Application.Shared.Persistence;
using _116.Identity.Application.Shared.Repositories;
using _116.Identity.Domain.Entities;
using _116.Identity.Domain.Enums;
using _116.Identity.Domain.ValueObjects;

namespace _116.Identity.Application.Auth.UseCases.Public.Commands.ResetPassword;

/// <summary>
/// Factory implementation for handling user password reset logic. The security email, in-app
/// notification react to the domain event the aggregate raises when the password changes. The
/// reset itself carries no acting session, so the new hash and the revocation of every session of
/// the account commit together: a stolen refresh token cannot survive its owner's reset.
/// </summary>
/// <param name="authRepository">Repository for user data access operations.</param>
/// <param name="passwordService">Service for password hashing operations.</param>
/// <param name="sessionRepository">Repository revoking the user's sessions.</param>
/// <param name="unitOfWork">Unit of Work for managing database transactions.</param>
/// <param name="userErrors">User domain error factory for generating domain exceptions.</param>
public class PublicResetPasswordAuthFactory(
    IAuthRepository authRepository,
    IPasswordService passwordService,
    ISessionRepository sessionRepository,
    IIdentityUnitOfWork unitOfWork,
    UserErrors userErrors
) : IPublicResetPasswordAuthFactory
{
    /// <summary>
    /// Gets and validates user by email for password reset.
    /// </summary>
    public async Task<PublicResetPasswordAuthData> GetUserForResetAsync(
        string email,
        CancellationToken cancellationToken
    )
    {
        var emailValue = new Email(value: email);
        UserEntity? user = await authRepository.GetUserWithRolesByEmailOrThrow(
            email: emailValue,
            cancellationToken: cancellationToken
        );

        authRepository.IsUserAccountActive(user!);
        authRepository.IsUserAccountVerified(user!);

        return new PublicResetPasswordAuthData(User: user!);
    }

    /// <summary>
    /// Resets a user's password with a new password.
    /// </summary>
    public async Task<UserEntity> ResetPasswordAsync(
        UserEntity user,
        string newPassword,
        CancellationToken cancellationToken
    )
    {
        if (passwordService.Verify(password: newPassword, hash: user.PasswordHash))
        {
            throw userErrors.NewPasswordSameAsOld();
        }

        string hashedPassword = passwordService.Hash(password: newPassword);
        user.UpdatePassword(
            newPasswordHash: hashedPassword,
            errors: userErrors,
            origin: EnumPasswordChangeOrigin.Reset
        );

        await sessionRepository.DeleteAllByUserIdAsync(
            userId: user.Id,
            reason: EnumSessionRevokeReason.SecurityInvalidation,
            exemptSessionId: null,
            cancellationToken: cancellationToken
        );

        await unitOfWork.CommitAsync(cancellationToken: cancellationToken);

        return user;
    }
}
