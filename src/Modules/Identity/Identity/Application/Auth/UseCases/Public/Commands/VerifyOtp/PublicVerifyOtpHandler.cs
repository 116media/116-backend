using _116.Identity.Application.Auth.Factories.Contracts;
using _116.Identity.Application.Auth.Repositories;
using _116.Identity.Application.Shared.Errors.Facade;
using _116.Identity.Application.Shared.Persistence;
using _116.Identity.Application.Shared.Repositories;
using _116.Identity.Domain.Entities;
using _116.Identity.Domain.Enums;
using _116.Identity.Domain.ValueObjects;
using _116.Shared.Application.Exceptions;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Identity.Application.Auth.UseCases.Public.Commands.VerifyOtp;

/// <summary>
/// Handles the <see cref="PublicVerifyOtpCommand" /> to verify OTP codes for user account verification.
/// The welcome email reacts to the domain event the user aggregate raises when it transitions to
/// verified.
/// </summary>
/// <param name="authRepository">Repository for user data access operations.</param>
/// <param name="otpRepository">Repository for OTP data access operations.</param>
/// <param name="otpVerificationFactory">Factory validating the presented code and metering misses.</param>
/// <param name="lockoutRepository">Repository clearing the failure counter on success.</param>
/// <param name="unitOfWork">Unit of Work for managing database transactions.</param>
/// <param name="timeProvider">Clock supplying the instant the code is judged against.</param>
/// <param name="i18n">Single i18n entry point for the Identity module.</param>
public class PublicVerifyOtpHandler(
    IAuthRepository authRepository,
    IOtpRepository otpRepository,
    IOtpVerificationFactory otpVerificationFactory,
    IAccountLockoutRepository lockoutRepository,
    IIdentityUnitOfWork unitOfWork,
    TimeProvider timeProvider,
    IdentityI18n i18n
) : ICommandHandler<PublicVerifyOtpCommand, PublicVerifyOtpResult>
{
    /// <summary>
    /// Handles the OTP verification command by validating the code and updating user verification status.
    /// </summary>
    /// <param name="command">The OTP verification command containing email and code.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A <see cref="PublicVerifyOtpResult" /> containing verification status and message.</returns>
    /// <exception cref="NotFoundException">Thrown when no user or outstanding OTP is found.</exception>
    /// <exception cref="ConflictException">Thrown when the account is already verified.</exception>
    /// <exception cref="BadRequestException">Thrown when OTP code is invalid.</exception>
    /// <exception cref="AuthenticationException">Thrown when OTP is expired.</exception>
    /// <exception cref="AuthorizationException">Thrown when max attempts are reached.</exception>
    public async Task<PublicVerifyOtpResult> Handle(PublicVerifyOtpCommand command, CancellationToken cancellationToken)
    {
        var email = new Email(value: command.Email);
        var purpose = new OtpPurpose(value: command.Purpose);
        UserEntity? user = await authRepository.GetUserWithRolesByEmailOrThrow(
            email: email,
            cancellationToken: cancellationToken
        );

        // Check if user is already verified (only for email verification purpose)
        if (user!.IsVerified && purpose.Value == EnumOtpPurpose.EmailVerification)
        {
            throw i18n.User.AccountAlreadyVerified();
        }

        OtpEntity otp = await otpRepository.GetLatestOutstandingOtpOrThrowAsync(
            userId: user.Id,
            purpose: purpose,
            cancellationToken: cancellationToken
        );

        await otpVerificationFactory.ValidateOtpAsync(
            otp: otp,
            code: command.Code,
            userId: user.Id,
            cancellationToken: cancellationToken
        );

        otp.MarkAsUsed(now: timeProvider.GetUtcNow().UtcDateTime);
        user.MarkVerifiedByOtp(purpose: purpose);

        // Invalidate any remaining OTPs for this purpose
        await otpRepository.InvalidateExistingOtpsAsync(
            userId: user.Id,
            purpose: purpose,
            exceptOtpId: otp.Id,
            cancellationToken: cancellationToken
        );
        await lockoutRepository.ClearFailedOtpAsync(userId: user.Id, cancellationToken: cancellationToken);
        await unitOfWork.CommitAsync(cancellationToken: cancellationToken);

        return new PublicVerifyOtpResult(IsSuccess: true);
    }
}
