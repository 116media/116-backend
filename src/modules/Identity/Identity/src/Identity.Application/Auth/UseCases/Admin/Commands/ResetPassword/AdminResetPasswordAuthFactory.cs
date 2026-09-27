using _116.Identity.Application.Auth.Services;
using _116.Identity.Application.Auth.UseCases.Admin.Commands.ResetPassword.Contracts;
using _116.Identity.Application.Session.Repositories;
using _116.Identity.Application.Shared.Errors;
using _116.Identity.Application.Shared.Persistence;
using _116.Identity.Application.Shared.Repositories;
using _116.Identity.Domain.Entities;
using _116.Identity.Domain.Enums;
using _116.Identity.Domain.ValueObjects;

namespace _116.Identity.Application.Auth.UseCases.Admin.Commands.ResetPassword;

/// <summary>
/// Factory implementation for handling admin user password reset logic. The security email, in-app
/// notification react to the domain event the aggregate raises when the password changes. The
/// reset itself carries no acting session, so the new hash and the revocation of every session of
/// the account commit together: a stolen refresh token cannot survive its owner's reset.
/// </summary>
/// <param name="authRepository">Repository for user data access operations.</param>
/// <param name="passwordService">Service for password hashing operations.</param>
/// <param name="sessionRepository">Repository revoking the user's sessions.</param>
/// <param name="tokenStateRepository">Repository rotating the user's security stamp.</param>
/// <param name="unitOfWork">Unit of Work for managing database transactions.</param>
/// <param name="userErrors">User domain error factory for generating domain exceptions.</param>
public class AdminResetPasswordAuthFactory(
    IAuthRepository authRepository,
    IPasswordService passwordService,
    ISessionRepository sessionRepository,
    IUserTokenStateRepository tokenStateRepository,
    IIdentityUnitOfWork unitOfWork,
    UserErrors userErrors
) : IAdminResetPasswordAuthFactory
{
    /// <summary>
    /// Gets and validates admin user by email for password reset.
    /// </summary>
    public async Task<AdminResetPasswordAuthData> GetUserForResetAsync(
        string email,
        CancellationToken cancellationToken
    )
    {
        var emailValue = new Email(value: email);
        UserEntity? user = await authRepository.GetUserWithRolesByEmailOrThrow(
            email: emailValue,
            cancellationToken: cancellationToken
        );

        authRepository.IsUserAdmin(user!);
        authRepository.IsUserAccountActive(user!);

        return new AdminResetPasswordAuthData(User: user!);
    }

    /// <summary>
    /// Resets an admin user's password with a new password.
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
        user.UpdatePassword(newPasswordHash: hashedPassword, origin: EnumPasswordChangeOrigin.Reset);

        await sessionRepository.DeleteAllByUserIdAsync(
            userId: user.Id,
            reason: EnumSessionRevokeReason.SecurityInvalidation,
            exemptSessionId: null,
            cancellationToken: cancellationToken
        );

        await unitOfWork.CommitAsync(cancellationToken: cancellationToken);

        await tokenStateRepository.RotateSecurityStampAsync(userId: user.Id, cancellationToken: cancellationToken);

        return user;
    }
}
