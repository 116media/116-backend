using _116.Identity.Application.Auth.Repositories;
using _116.Identity.Application.Auth.UseCases.Public.Commands.ResetPassword.Contracts;
using _116.Identity.Domain.Enums;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Identity.Application.Auth.UseCases.Public.Commands.ResetPassword;

/// <summary>
/// Handles the <see cref="PublicResetPasswordCommand" /> to reset user password using OTP verification.
/// </summary>
/// <param name="authFactory">Factory for handling user password reset logic.</param>
/// <param name="otpRepository">Repository for OTP data access operations.</param>
public class PublicResetPasswordHandler(
    IPublicResetPasswordAuthFactory authFactory,
    IOtpRepository otpRepository
) : ICommandHandler<PublicResetPasswordCommand, PublicResetPasswordResult>
{
    /// <summary>
    /// Handles the password reset command by validating OTP and updating the user's password.
    /// </summary>
    /// <param name="command">The password reset command containing email, OTP, and new password.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A <see cref="PublicResetPasswordResult" /> containing reset status.</returns>
    /// <exception cref="NotFoundException">Thrown when no user is found with the specified email.</exception>
    /// <exception cref="BadRequestException">Thrown when the account is not active or verified.</exception>
    /// <exception cref="NotFoundException">Thrown when no valid OTP is found.</exception>
    /// <exception cref="BadRequestException">Thrown when OTP code is invalid.</exception>
    /// <exception cref="AuthenticationException">Thrown when OTP is expired.</exception>
    /// <exception cref="AuthorizationException">Thrown when max attempts are reached.</exception>
    public async Task<PublicResetPasswordResult> Handle(
        PublicResetPasswordCommand command,
        CancellationToken cancellationToken
    )
    {
        PublicResetPasswordAuthData authData = await authFactory.GetUserForResetAsync(
            email: command.Email,
            cancellationToken: cancellationToken
        );

        await otpRepository.ValidateUsedOtpAsync(
            code: command.Code,
            userId: authData.User.Id,
            purpose: EnumOtpPurpose.PasswordReset,
            cancellationToken: cancellationToken
        );

        await authFactory.ResetPasswordAsync(
            user: authData.User,
            newPassword: command.NewPassword,
            cancellationToken: cancellationToken
        );

        return new PublicResetPasswordResult(true);
    }
}
