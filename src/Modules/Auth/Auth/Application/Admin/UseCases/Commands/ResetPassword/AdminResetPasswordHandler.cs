using _116.Shared.Application.Exceptions;
using _116.Shared.Contracts.Application.CQRS;
using _116.Auth.Application.Shared.Repositories;
using _116.Auth.Application.Shared.Services;
using _116.Auth.Domain.Entities;
using _116.Auth.Domain.ValueObjects;
using OtpPurpose = _116.Auth.Domain.Enums.OtpPurpose;

namespace _116.Auth.Application.Admin.UseCases.Commands.ResetPassword;

/// <summary>
/// Handles the <see cref="AdminResetPasswordCommand"/> to reset admin user password using OTP verification.
/// </summary>
/// <param name="userRepository">Repository for user data access operations.</param>
/// <param name="otpRepository">Repository for OTP data access operations.</param>
/// <param name="passwordService">Service for password hashing operations.</param>
public class AdminResetPasswordHandler(
    IUserRepository userRepository,
    IOtpRepository otpRepository,
    IPasswordService passwordService
) : ICommandHandler<AdminResetPasswordCommand, AdminResetPasswordResult>
{
    /// <summary>
    /// Handles the password reset command by validating OTP and updating the admin user's password.
    /// </summary>
    /// <param name="command">The password reset command containing email, OTP, and new password.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A <see cref="AdminResetPasswordResult"/> containing reset status.</returns>
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
        // Normalize email using value object
        var email = new Email(command.Email);

        // Get admin user by email
        UserEntity user = await userRepository.GetActiveAdminUserWithRolesAndPermissionsAsync(email, cancellationToken);

        // Validate the OTP for password reset (throws appropriate exceptions on failure)
        OtpEntity otp = await otpRepository.ValidateOtpAsync(
            user.Id,
            command.Code,
            OtpPurpose.PasswordReset,
            cancellationToken
        );

        // Hash the new password
        string hashedPassword = passwordService.Hash(command.NewPassword);

        // Update user's password
        user.UpdatePassword(hashedPassword);
        await userRepository.UpdateAsync(user, cancellationToken);

        // Mark OTP as used
        otp.MarkAsUsed();
        await otpRepository.UpdateAsync(otp, cancellationToken);

        // Invalidate any remaining password reset OTPs for this user
        await otpRepository.InvalidateExistingOtpsAsync(
            user.Id,
            OtpPurpose.PasswordReset,
            cancellationToken
        );

        // Save all changes
        await otpRepository.SaveChangesAsync(cancellationToken);

        return new AdminResetPasswordResult(
            IsSuccess: true
        );
    }
}
