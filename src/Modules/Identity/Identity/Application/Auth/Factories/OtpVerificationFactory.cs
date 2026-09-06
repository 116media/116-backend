using _116.Identity.Application.Auth.Factories.Contracts;
using _116.Identity.Application.Auth.Services;
using _116.Identity.Application.Shared.Errors.Facade;
using _116.Identity.Application.Shared.Persistence;
using _116.Identity.Application.Shared.Repositories;
using _116.Identity.Domain.Entities;
using _116.Identity.Domain.Enums;

namespace _116.Identity.Application.Auth.Factories;

/// <summary>
/// Validates a presented OTP code, meters a missed attempt against the account, and commits it
/// before throwing so the counter survives the error response.
/// </summary>
/// <param name="otpService">Service comparing the presented code against the stored keyed hash.</param>
/// <param name="lockoutRepository">Repository metering failures against the account.</param>
/// <param name="unitOfWork">Unit of Work committing the consumed attempt.</param>
/// <param name="timeProvider">Clock supplying the instant the code is verified against.</param>
/// <param name="i18n">Single i18n entry point for the Identity module.</param>
public class OtpVerificationFactory(
    IOtpService otpService,
    IAccountLockoutRepository lockoutRepository,
    IIdentityUnitOfWork unitOfWork,
    TimeProvider timeProvider,
    IdentityI18n i18n
) : IOtpVerificationFactory
{
    /// <inheritdoc />
    public async Task ValidateOtpAsync(OtpEntity otp, string code, Guid userId, CancellationToken cancellationToken)
    {
        int attemptsBefore = otp.AttemptCount;
        EnumOtpVerificationStatus verificationStatus = otp.Verify(
            suppliedCodeMatches: otpService.Verify(code: code, hash: otp.CodeHash),
            now: timeProvider.GetUtcNow().UtcDateTime
        );

        if (verificationStatus == EnumOtpVerificationStatus.Valid)
        {
            return;
        }

        // A missed code consumed an attempt on the OTP row and is metered against the account;
        // committing before the throw keeps both durable.
        if (otp.AttemptCount != attemptsBefore)
        {
            await lockoutRepository.RegisterFailedOtpAsync(userId: userId, cancellationToken: cancellationToken);
            await unitOfWork.CommitAsync(cancellationToken: cancellationToken);
        }

        throw verificationStatus switch
        {
            EnumOtpVerificationStatus.Expired => i18n.User.OtpExpired(),
            EnumOtpVerificationStatus.AttemptsExhausted => i18n.User.MaxOtpAttemptsReached(),
            _ => i18n.User.InvalidOtpCode(),
        };
    }
}
