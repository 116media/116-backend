using _116.Identity.Application.Auth.Ports;
using _116.Identity.Application.Auth.Repositories;
using _116.Identity.Application.Shared.Errors.Facade;
using _116.Identity.Application.Shared.Persistence;
using _116.Identity.Application.Shared.Repositories;
using _116.Identity.Domain.Entities;
using _116.Identity.Domain.Enums;
using _116.Identity.Domain.ValueObjects;

namespace _116.Identity.Application.Auth.Services;

/// <summary>
/// Consumes the outstanding OTP for an account: judges the presented code, meters and commits
/// a missed attempt before throwing, and on success marks the code used, invalidates its
/// siblings and clears the account failure counter.
/// </summary>
/// <param name="otpRepository">Repository for OTP data access operations.</param>
/// <param name="otpService">Service comparing the presented code against the stored keyed hash.</param>
/// <param name="lockoutRepository">Repository metering and clearing account failures.</param>
/// <param name="unitOfWork">Unit of Work committing the consumed attempt.</param>
/// <param name="timeProvider">Clock supplying the instant the code is verified against.</param>
/// <param name="i18n">Single i18n entry point for the Identity module.</param>
public class OtpVerificationService(
    IOtpRepository otpRepository,
    IOtpService otpService,
    IAccountLockoutRepository lockoutRepository,
    IIdentityUnitOfWork unitOfWork,
    TimeProvider timeProvider,
    IdentityI18n i18n
) : IOtpVerificationService
{
    /// <inheritdoc />
    public async Task ConsumeOtpAsync(Guid userId, OtpPurpose purpose, string code, CancellationToken cancellationToken)
    {
        OtpEntity otp = await otpRepository.GetLatestOutstandingOtpOrThrowAsync(
            userId: userId,
            purpose: purpose,
            cancellationToken: cancellationToken
        );

        int attemptsBefore = otp.AttemptCount;
        EnumOtpVerificationStatus verificationStatus = otp.Verify(
            suppliedCodeMatches: otpService.Verify(code: code, hash: otp.CodeHash),
            now: timeProvider.GetUtcNow().UtcDateTime
        );

        if (verificationStatus != EnumOtpVerificationStatus.Valid)
        {
            // A missed code consumed an attempt on the OTP row and is metered against the account.
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

        otp.MarkAsUsed(now: timeProvider.GetUtcNow().UtcDateTime);
        await otpRepository.InvalidateExistingOtpsAsync(
            userId: userId,
            purpose: purpose,
            exceptOtpId: otp.Id,
            cancellationToken: cancellationToken
        );
        await lockoutRepository.ClearFailedOtpAsync(userId: userId, cancellationToken: cancellationToken);
    }
}
