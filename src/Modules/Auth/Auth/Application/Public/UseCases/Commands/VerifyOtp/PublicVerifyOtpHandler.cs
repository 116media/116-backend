using _116.Shared.Application.Exceptions;
using _116.Shared.Contracts.Application.CQRS;
using _116.Auth.Application.Shared.Errors;
using _116.Auth.Application.Shared.Repositories;
using _116.Auth.Domain.Entities;
using _116.Auth.Domain.ValueObjects;

namespace _116.Auth.Application.Public.UseCases.Commands.VerifyOtp;

/// <summary>
/// Handles the <see cref="PublicVerifyOtpCommand"/> to verify OTP codes for user account verification.
/// </summary>
/// <param name="userRepository">Repository for user data access operations.</param>
/// <param name="otpRepository">Repository for OTP data access operations.</param>
public class PublicVerifyOtpHandler(
    IUserRepository userRepository,
    IOtpRepository otpRepository
) : ICommandHandler<PublicVerifyOtpCommand, PublicVerifyOtpResult>
{
    /// <summary>
    /// Handles the OTP verification command by validating the code and updating user verification status.
    /// </summary>
    /// <param name="command">The OTP verification command containing email and code.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A <see cref="PublicVerifyOtpResult"/> containing verification status and message.</returns>
    /// <exception cref="NotFoundException">Thrown when no user is found with the specified email.</exception>
    /// <exception cref="ConflictException">Thrown when the account is already verified.</exception>
    /// <exception cref="NotFoundException">Thrown when no valid OTP is found.</exception>
    /// <exception cref="BadRequestException">Thrown when OTP code is invalid.</exception>
    /// <exception cref="AuthenticationException">Thrown when OTP is expired.</exception>
    /// <exception cref="AuthorizationException">Thrown when max attempts are reached.</exception>
    public async Task<PublicVerifyOtpResult> Handle(PublicVerifyOtpCommand command, CancellationToken cancellationToken)
    {
        // Normalize email and purpose using value objects
        var email = new Email(command.Email);
        var purpose = new OtpPurpose(command.Purpose);

        // Get user by email
        UserEntity user = await userRepository.GetUserWithRolesOrThrowAsync(email, cancellationToken);

        // Check if user is already verified
        if (user.IsVerified)
        {
            throw UserErrors.AccountAlreadyVerified();
        }

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

        user.MarkAsVerified();
        await userRepository.UpdateAsync(user, cancellationToken);

        // Invalidate any remaining OTPs for this purpose
        await otpRepository.InvalidateExistingOtpsAsync(
            user.Id,
            purpose,
            cancellationToken
        );

        await otpRepository.SaveChangesAsync(cancellationToken);

        return new PublicVerifyOtpResult(
            IsSuccess: true
        );
    }
}
