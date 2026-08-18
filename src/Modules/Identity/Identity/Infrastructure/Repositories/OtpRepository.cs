using _116.BuildingBlocks.Constants;
using _116.Identity.Application.Auth.Repositories;
using _116.Identity.Application.Auth.Services;
using _116.Identity.Application.Auth.Specifications;
using _116.Identity.Application.Shared.Errors;
using _116.Identity.Application.Shared.Repositories;
using _116.Identity.Domain.Entities;
using _116.Identity.Domain.Enums;
using _116.Identity.Infrastructure.Persistence;
using _116.Shared.Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;

namespace _116.Identity.Infrastructure.Repositories;

/// <summary>
/// Implementation of <see cref="IOtpRepository" /> using Entity Framework Core.
/// </summary>
/// <param name="context">The identity database context.</param>
/// <param name="userErrors">User domain error factory for generating localized domain exceptions.</param>
/// <param name="otpHasher">
/// Keyed hasher used to compare a supplied code against the stored hash. The pepper cannot be
/// pushed into the query, so the comparison happens once a candidate row is loaded.
/// </param>
/// <param name="lockoutRepository">Repository recording failed OTP attempts against the account.</param>
public class OtpRepository(
    IdentityDbContext context,
    UserErrors userErrors,
    IOtpHasher otpHasher,
    IAccountLockoutRepository lockoutRepository
) : IOtpRepository
{
    /// <inheritdoc />
    public async Task AddAsync(OtpEntity otp, CancellationToken cancellationToken = default)
    {
        await context.Otps.AddAsync(entity: otp, cancellationToken: cancellationToken);
    }

    /// <inheritdoc />
    public async Task<OtpEntity> ValidateOtpAsync(
        Guid userId,
        string code,
        EnumOtpPurpose purpose,
        CancellationToken cancellationToken = default
    )
    {
        // Load the outstanding OTP for this user and purpose; the code itself cannot take part
        // in the query because the stored value is salted.
        var specification = new OtpForValidationSpecification(userId: userId, purpose: purpose);
        OtpEntity? candidateOtp = await context
            .Otps.ApplySpecification(specification: specification)
            .OrderByDescending(o => o.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken: cancellationToken);

        if (candidateOtp == null)
        {
            throw userErrors.NoValidOtpFound();
        }

        // First check if the otp is not expired
        if (candidateOtp.IsExpired())
        {
            throw userErrors.OtpExpired();
        }

        // Then check if the max attempts are not reached
        if (candidateOtp.HasMaxAttemptsReached())
        {
            throw userErrors.MaxOtpAttemptsReached();
        }

        // Then compare the supplied code against the stored hash
        if (otpHasher.Verify(code: code, hash: candidateOtp.CodeHash))
        {
            return candidateOtp;
        }

        // Now increment both the per-code allowance and the account counter that survives a resend
        await lockoutRepository.RegisterFailedOtpAsync(userId: userId, cancellationToken: cancellationToken);
        candidateOtp.IncrementAttemptCount();

        context.Otps.Update(entity: candidateOtp);
        await context.SaveChangesAsync(cancellationToken: cancellationToken);

        if (candidateOtp.HasMaxAttemptsReached())
        {
            throw userErrors.MaxOtpAttemptsReached();
        }

        throw userErrors.InvalidOtpCode();
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
        OtpEntity? matchingOtp = await context
            .Otps.ApplySpecification(specification: specification)
            .OrderByDescending(o => o.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken: cancellationToken);

        // Check if a verified OTP exists and that it is the one the caller presented. A miss is
        // metered against the account, so guessing here costs the same as guessing at verify-otp.
        if (matchingOtp == null || !otpHasher.Verify(code: code, hash: matchingOtp.CodeHash))
        {
            await lockoutRepository.RegisterFailedOtpAsync(userId: userId, cancellationToken: cancellationToken);
            throw userErrors.OtpNotYetVerified();
        }

        // Check if the OTP has expired
        if (matchingOtp.IsExpired())
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
        DateTime windowStart = DateTime.UtcNow.AddMinutes(value: -UserConstants.OtpResendWindowMinutes);

        return await context.Otps.CountAsync(
            o => o.UserId == userId && o.Purpose == purpose && o.CreatedAt >= windowStart,
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
        List<OtpEntity> expiredOtpList = await context
            .Otps.ApplySpecification(specification: specification)
            .Where(o => exceptOtpId == null || o.Id != exceptOtpId)
            .ToListAsync(cancellationToken: cancellationToken);

        foreach (OtpEntity otp in expiredOtpList)
        {
            otp.MarkAsConsumed();
        }
    }

    /// <inheritdoc />
    public async Task<int> CleanupExpiredOtpsAsync(CancellationToken cancellationToken = default)
    {
        var specification = new OtpIsExpiredSpecification();
        List<OtpEntity> expiredOtpList = await context
            .Otps.ApplySpecification(specification: specification)
            .ToListAsync(cancellationToken: cancellationToken);

        context.Otps.RemoveRange(entities: expiredOtpList);
        return expiredOtpList.Count;
    }
}
