using _116.Identity.Application.Auth.Services;
using _116.Identity.Application.Session.Repositories;
using _116.Identity.Application.Shared.Errors.Facade;
using _116.Identity.Application.Shared.Persistence;
using _116.Identity.Application.Shared.Repositories;
using _116.Identity.Domain.Entities;
using _116.Identity.Domain.Enums;
using _116.Shared.Application.Exceptions;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Identity.Application.Auth.UseCases.Admin.Commands.ChangePassword;

/// <summary>
/// Handles the <see cref="AdminChangePasswordCommand" /> to change admin user password with current password
/// verification. The new hash and the revocation of the user's other sessions commit together, so
/// the old credential can never outlive the change. The security email and in-app notification
/// react to the domain event the aggregate raises when the password changes.
/// </summary>
/// <param name="authRepository">Repository for user data access operations.</param>
/// <param name="passwordService">Service for password hashing and verification operations.</param>
/// <param name="sessionRepository">Repository revoking the user's sessions.</param>
/// <param name="tokenStateRepository">Repository rotating the user's security stamp.</param>
/// <param name="unitOfWork">Unit of Work for managing database transactions.</param>
/// <param name="i18n">Single i18n entry point for the Identity module.</param>
public class AdminChangePasswordHandler(
    IAuthRepository authRepository,
    IPasswordService passwordService,
    ISessionRepository sessionRepository,
    IUserTokenStateRepository tokenStateRepository,
    IIdentityUnitOfWork unitOfWork,
    IdentityI18n i18n
) : ICommandHandler<AdminChangePasswordCommand, AdminChangePasswordResult>
{
    /// <summary>
    /// Handles the password change command by verifying the old password and updating to the new password.
    /// </summary>
    /// <param name="command">The password change command containing user ID, old password, and new password.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A <see cref="AdminChangePasswordResult" /> containing change status.</returns>
    /// <exception cref="NotFoundException">Thrown when no user is found with the specified ID.</exception>
    /// <exception cref="BadRequestException">Thrown when the account is not active.</exception>
    /// <exception cref="BadRequestException">Thrown when old password is invalid.</exception>
    /// <exception cref="ConflictException">Thrown when new password is the same as old password.</exception>
    /// <exception cref="BadRequestException">Thrown when the current password is incorrect.</exception>
    public async Task<AdminChangePasswordResult> Handle(
        AdminChangePasswordCommand command,
        CancellationToken cancellationToken
    )
    {
        UserEntity? user = await authRepository.FindUserByIdOrThrow(
            userId: command.UserId,
            cancellationToken: cancellationToken
        );

        authRepository.IsUserAccountActive(user!);
        await authRepository.IsSessionValidAsync(command.SessionId, cancellationToken);

        // Verify old password
        if (!passwordService.Verify(password: command.OldPassword, hash: user!.PasswordHash))
        {
            throw i18n.User.IncorrectCurrentPassword();
        }

        // Check if new password is different from old password
        if (passwordService.Verify(password: command.NewPassword, hash: user.PasswordHash))
        {
            throw i18n.User.NewPasswordSameAsOld();
        }

        string hashedNewPassword = passwordService.Hash(password: command.NewPassword);
        user.UpdatePassword(newPasswordHash: hashedNewPassword, origin: EnumPasswordChangeOrigin.Changed);

        // The acting session survives its own password change; every other session of the
        // account loses the credential in the same transaction as the new hash.
        await sessionRepository.DeleteAllByUserIdAsync(
            userId: user.Id,
            reason: EnumSessionRevokeReason.SecurityInvalidation,
            exemptSessionId: command.SessionId,
            cancellationToken: cancellationToken
        );

        await unitOfWork.CommitAsync(cancellationToken: cancellationToken);

        await tokenStateRepository.RotateSecurityStampAsync(userId: user.Id, cancellationToken: cancellationToken);

        return new AdminChangePasswordResult(IsSuccess: true);
    }
}
