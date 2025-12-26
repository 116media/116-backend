using _116.Identity.Application.Auth.Repositories;
using _116.Identity.Application.Shared.Persistence;
using _116.Identity.Application.Shared.Repositories;
using _116.Identity.Domain.Entities;
using _116.Identity.Domain.ValueObjects;
using _116.Shared.Application.Exceptions;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Identity.Application.Auth.UseCases.Admin.Commands.VerifyOtp;

/// <summary>
/// Handles the <see cref="AdminVerifyOtpCommand" /> to verify OTP codes for admin account verification.
/// </summary>
/// <param name="authRepository">Repository for user data access operations.</param>
/// <param name="otpRepository">Repository for OTP data access operations.</param>
/// <param name="unitOfWork">Unit of Work for managing database transactions.</param>
public class AdminVerifyOtpHandler(
    IAuthRepository authRepository,
    IOtpRepository otpRepository,
    IIdentityUnitOfWork unitOfWork
) : ICommandHandler<AdminVerifyOtpCommand, AdminVerifyOtpResult>
{
    /// <summary>
    /// Handles the OTP verification command by validating the code and updating admin user verification status.
    /// </summary>
    /// <param name="command">The OTP verification command containing email and code.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A <see cref="AdminVerifyOtpResult" /> containing verification status and message.</returns>
    /// <exception cref="NotFoundException">Thrown when no admin user is found with the specified email.</exception>
    /// <exception cref="ConflictException">Thrown when the account is already verified.</exception>
    /// <exception cref="NotFoundException">Thrown when no valid OTP is found.</exception>
    /// <exception cref="BadRequestException">Thrown when OTP code is invalid.</exception>
    /// <exception cref="AuthenticationException">Thrown when OTP is expired.</exception>
    /// <exception cref="AuthorizationException">Thrown when max attempts are reached.</exception>
    public async Task<AdminVerifyOtpResult> Handle(AdminVerifyOtpCommand command, CancellationToken cancellationToken)
    {
        // Normalize email and purpose using value objects
        var email = new Email(value: command.Email);
        var purpose = new OtpPurpose(value: command.Purpose);
        UserEntity? user =
            await authRepository.GetUserWithRolesByEmailOrThrow(email: email, cancellationToken: cancellationToken);
        // Validate admin account status
        authRepository.IsUserAdmin(user!);
        authRepository.IsUserAccountActive(user!);
        // Validate the OTP (throws appropriate exceptions on failure)
        OtpEntity otp = await otpRepository.ValidateOtpAsync(
            userId: user!.Id,
            code: command.Code,
            purpose: purpose,
            cancellationToken: cancellationToken
        );
        // Mark OTP as used and user as verified
        otp.MarkAsUsed();
        user.MarkAsVerified();
        // Invalidate any remaining OTPs for this purpose
        await otpRepository.InvalidateExistingOtpsAsync(
            userId: user.Id,
            purpose: purpose,
            cancellationToken: cancellationToken
        );
        await unitOfWork.CommitAsync(cancellationToken: cancellationToken);
        return new AdminVerifyOtpResult(
            true
        );
    }
}
