using _116.BuildingBlocks.Constants;
using _116.Identity.Application.Auth.Repositories;
using _116.Identity.Application.Auth.Services;
using _116.Identity.Application.Auth.Specifications;
using _116.Identity.Application.Shared.Errors;
using _116.Identity.Application.Shared.Repositories;
using _116.Identity.Domain.Entities;
using _116.Identity.Domain.Enums;
using _116.Identity.Domain.ValueObjects;
using _116.Identity.Infrastructure.Persistence;
using _116.Shared.Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;

namespace _116.Identity.Infrastructure.Repositories;

/// <summary>
/// Implementation of <see cref="IOtpRepository" /> using Entity Framework Core.
/// </summary>
/// <param name="context">The identity database context.</param>
/// <param name="userErrors">User domain error factory for generating localized domain exceptions.</param>
/// <param name="otpService">
/// Service whose keyed hashing compares a supplied code against the stored hash. The pepper cannot
/// be pushed into the query, so the comparison happens once a candidate row is loaded.
/// </param>
/// <param name="lockoutRepository">Repository recording failed OTP attempts against the account.</param>
/// <param name="timeProvider">Clock supplying the instant expiry judgements are made against.</param>
public class OtpRepository(
    IdentityDbContext context,
    UserErrors userErrors,
    IOtpService otpService,
    IAccountLockoutRepository lockoutRepository,
    TimeProvider timeProvider
) : IdentityRepository<OtpEntity>(context), IOtpRepository
{
    /// <inheritdoc />
    public async Task<OtpEntity> GetLatestOutstandingOtpOrThrowAsync(
        Guid userId,
        EnumOtpPurpose purpose,
        CancellationToken cancellationToken = default
    )
    {
        // The code itself cannot take part in the query because the stored value is salted;
        // judging the presented code against this row is the domain's job (OtpEntity.Verify).
        var specification = new OtpForValidationSpecification(userId: userId, purpose: purpose);
        OtpEntity? candidateOtp = await Context
            .Otps.ApplySpecification(specification: specification)
            .OrderByDescending(o => o.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken: cancellationToken);

        if (candidateOtp == null)
        {
            throw userErrors.NoValidOtpFound();
        }

        return candidateOtp;
    }

    /// <inheritdoc />
    public async Task<OtpEntity> ValidateUsedOtpAsync(
        Guid userId,
        string code,
        EnumOtpPurpose purpose,
        CancellationToken cancellationToken = default
    )
    {
        // Load the most recently consumed OTP for this user and purpose; the salted hash keeps
        // the code out of the query, so the comparison happens on the loaded row.
        var specification = new OtpForUsedValidationSpecification(userId: userId, purpose: purpose);
        OtpEntity? matchingOtp = await Context
            .Otps.ApplySpecification(specification: specification)
            .OrderByDescending(o => o.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken: cancellationToken);

        // Check if a verified OTP exists and that it is the one the caller presented. A miss is
        // metered against the account, so guessing here costs the same as guessing at verify-otp.
        if (matchingOtp == null || !otpService.Verify(code: code, hash: matchingOtp.CodeHash))
        {
            await lockoutRepository.RegisterFailedOtpAsync(userId: userId, cancellationToken: cancellationToken);
            throw userErrors.OtpNotYetVerified();
        }

        // Check if the OTP has expired
        if (matchingOtp.IsExpired(now: timeProvider.GetUtcNow().UtcDateTime))
        {
            throw userErrors.OtpExpired();
        }

        return matchingOtp;
    }

    /// <inheritdoc />
    public async Task<int> CountRecentOtpsAsync(
        Guid userId,
        EnumOtpPurpose purpose,
        CancellationToken cancellationToken = default
    )
    {
        DateTime windowStart = timeProvider
            .GetUtcNow()
            .UtcDateTime.AddMinutes(value: -UserConstants.OtpResendWindowMinutes);

        var target = new OtpPurpose(value: purpose);
        return await Context.Otps.CountAsync(
            o => o.UserId == userId && o.Purpose == target && o.CreatedAt >= windowStart,
            cancellationToken: cancellationToken
        );
    }

    /// <inheritdoc />
    public async Task InvalidateExistingOtpsAsync(
        Guid userId,
        EnumOtpPurpose purpose,
        Guid? exceptOtpId = null,
        CancellationToken cancellationToken = default
    )
    {
        var specification = new OtpForInvalidationSpecification(userId: userId, purpose: purpose);

        // The redeemed code is only marked used in memory at this point, so it still matches the
        // not-used predicate; excluding it by id stops verification consuming its own code.
        List<OtpEntity> expiredOtpList = await Context
            .Otps.ApplySpecification(specification: specification)
            .Where(o => exceptOtpId == null || o.Id != exceptOtpId)
            .ToListAsync(cancellationToken: cancellationToken);

        DateTime now = timeProvider.GetUtcNow().UtcDateTime;
        foreach (OtpEntity otp in expiredOtpList)
        {
            otp.MarkAsConsumed(now: now);
        }
    }

    /// <inheritdoc />
    public async Task<int> CleanupExpiredOtpsAsync(CancellationToken cancellationToken = default)
    {
        var specification = new OtpIsExpiredSpecification();
        List<OtpEntity> expiredOtpList = await Context
            .Otps.ApplySpecification(specification: specification)
            .ToListAsync(cancellationToken: cancellationToken);

        Context.Otps.RemoveRange(entities: expiredOtpList);
        return expiredOtpList.Count;
    }
}
