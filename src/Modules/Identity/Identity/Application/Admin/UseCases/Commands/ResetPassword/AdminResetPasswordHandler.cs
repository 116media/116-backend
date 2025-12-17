using _116.Identity.Application.Shared.Errors;
using _116.Shared.Application.Exceptions;
using _116.Identity.Application.Shared.Persistence;
using _116.Shared.Contracts.Application.CQRS;
using _116.Identity.Application.Shared.Repositories;
using _116.Identity.Application.Shared.Services;
using _116.Identity.Domain.Entities;
using _116.Identity.Domain.Enums;
using _116.Identity.Domain.ValueObjects;

namespace _116.Identity.Application.Admin.UseCases.Commands.ResetPassword;

/// <summary>
/// Handles the <see cref="AdminResetPasswordCommand"/> to reset admin user password using OTP verification.
/// </summary>
/// <param name="userRepository">Repository for user data access operations.</param>
/// <param name="otpRepository">Repository for OTP data access operations.</param>
/// <param name="passwordService">Service for password hashing operations.</param>
/// <param name="unitOfWork">Unit of Work for managing database transactions.</param>
public class AdminResetPasswordHandler(
    IUserRepository userRepository,
    IOtpRepository otpRepository,
    IPasswordService passwordService,
    IAuthUnitOfWork unitOfWork
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

        UserEntity? user = await userRepository.GetUserWithRolesByEmailOrThrow(email, cancellationToken);

        // Validate user account status
        userRepository.IsUserAdmin(user!);
        userRepository.IsUserAccountActive(user!);

        // Validate the OTP was already used for password reset
        await otpRepository.ValidateUsedOtpAsync(
            user!.Id,
            command.Code,
            EnumOtpPurpose.PasswordReset,
            cancellationToken
        );

        // Check if new password is different from old password
        if (passwordService.Verify(command.NewPassword, user.PasswordHash))
        {
            throw UserErrors.NewPasswordSameAsOld();
        }

        // Hash the new password
        string hashedPassword = passwordService.Hash(command.NewPassword);

        // Update user's password
        user.UpdatePassword(hashedPassword);

        await unitOfWork.CommitAsync(cancellationToken);

        return new AdminResetPasswordResult(
            IsSuccess: true
        );
    }
}
