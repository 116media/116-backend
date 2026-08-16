using _116.Identity.Application.Auth.Repositories;
using _116.Identity.Application.Auth.UseCases.Admin.Commands.ResetPassword.Contracts;
using _116.Identity.Domain.Enums;
using _116.Shared.Application.Exceptions;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Identity.Application.Auth.UseCases.Admin.Commands.ResetPassword;

/// <summary>
/// Handles the <see cref="AdminResetPasswordCommand" /> to reset admin user password using OTP verification.
/// </summary>
/// <param name="authFactory">Factory for handling admin user password reset logic.</param>
/// <param name="otpRepository">Repository for OTP data access operations.</param>
public class AdminResetPasswordHandler(IAdminResetPasswordAuthFactory authFactory, IOtpRepository otpRepository)
    : ICommandHandler<AdminResetPasswordCommand, AdminResetPasswordResult>
{
    /// <summary>
    /// Handles the password reset command by validating OTP and updating the admin user's password.
    /// </summary>
    /// <param name="command">The password reset command containing email, OTP, and new password.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A <see cref="AdminResetPasswordResult" /> containing reset status.</returns>
    /// <exception cref="NotFoundException">Thrown when no user is found with the specified email.</exception>
    /// <exception cref="BadRequestException">Thrown when the account is not active.</exception>
    /// <exception cref="NotFoundException">Thrown when no valid OTP is found.</exception>
    /// <exception cref="BadRequestException">Thrown when OTP code is invalid.</exception>
    /// <exception cref="AuthenticationException">Thrown when OTP is expired.</exception>
    /// <exception cref="AuthorizationException">Thrown when max attempts are reached.</exception>
    public async Task<AdminResetPasswordResult> Handle(
        AdminResetPasswordCommand command,
        CancellationToken cancellationToken
    )
    {
        AdminResetPasswordAuthData authData = await authFactory.GetUserForResetAsync(
            email: command.Email,
            cancellationToken: cancellationToken
        );

        await otpRepository.ValidateUsedOtpAsync(
            userId: authData.User.Id,
            code: command.Code,
            purpose: EnumOtpPurpose.PasswordReset,
            cancellationToken: cancellationToken
        );

        await authFactory.ResetPasswordAsync(
            user: authData.User,
            newPassword: command.NewPassword,
            cancellationToken: cancellationToken
        );

        return new AdminResetPasswordResult(IsSuccess: true);
    }
}
