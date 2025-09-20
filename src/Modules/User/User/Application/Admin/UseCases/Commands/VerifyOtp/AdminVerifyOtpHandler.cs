using _116.Shared.Application.Exceptions;
using _116.Shared.Contracts.Application.CQRS;
using _116.User.Application.Shared.Repositories;
using _116.User.Domain.Entities;
using _116.User.Domain.ValueObjects;

namespace _116.User.Application.Admin.UseCases.Commands.VerifyOtp;

/// <summary>
/// Handles the <see cref="AdminVerifyOtpCommand"/> to verify OTP codes for admin account verification.
/// </summary>
/// <param name="userRepository">Repository for user data access operations.</param>
/// <param name="otpRepository">Repository for OTP data access operations.</param>
public class AdminVerifyOtpHandler(
    IUserRepository userRepository,
    IOtpRepository otpRepository
) : ICommandHandler<AdminVerifyOtpCommand, AdminVerifyOtpResult>
{
    /// <summary>
    /// Handles the OTP verification command by validating the code and updating admin user verification status.
    /// </summary>
    /// <param name="command">The OTP verification command containing email and code.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A <see cref="AdminVerifyOtpResult"/> containing verification status and message.</returns>
    /// <exception cref="NotFoundException">Thrown when no admin user is found with the specified email.</exception>
    /// <exception cref="ConflictException">Thrown when the account is already verified.</exception>
    /// <exception cref="NotFoundException">Thrown when no valid OTP is found.</exception>
    /// <exception cref="BadRequestException">Thrown when OTP code is invalid.</exception>
    /// <exception cref="AuthenticationException">Thrown when OTP is expired.</exception>
    /// <exception cref="AuthorizationException">Thrown when max attempts are reached.</exception>
    public async Task<AdminVerifyOtpResult> Handle(AdminVerifyOtpCommand command, CancellationToken cancellationToken)
    {
        // Normalize email and purpose using value objects
        var email = new Email(command.Email);
        var purpose = new OtpPurpose(command.Purpose);

        // Get admin user by email
        UserEntity user = await userRepository.GetUserWithRolesOrThrowAsync(email, cancellationToken);

        // Validate the OTP (throws appropriate exceptions on failure)
        OtpEntity otp = await otpRepository.ValidateOtpAsync(
            user.Id,
            command.Code,
            purpose,
            cancellationToken
        );

        // Mark OTP as used
        otp.MarkAsUsed();
        await otpRepository.UpdateAsync(otp, cancellationToken);

        // Mark user as verified
        user.MarkAsVerified();
        await userRepository.UpdateAsync(user, cancellationToken);

        // Invalidate any remaining OTPs for this purpose
        await otpRepository.InvalidateExistingOtpsAsync(
            user.Id,
            purpose,
            cancellationToken
        );

        await otpRepository.SaveChangesAsync(cancellationToken);

        return new AdminVerifyOtpResult(
            IsSuccess: true
        );
    }
}
