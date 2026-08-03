using _116.Identity.Application.Auth.Repositories;
using _116.Identity.Application.Auth.Services;
using _116.Identity.Application.Auth.Specifications;
using _116.Identity.Application.Shared.Errors;
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
/// <param name="passwordService">
/// Hashing service used to compare a supplied code against the stored hash. Codes are salted,
/// so the comparison cannot be pushed into the query and happens once a candidate row is loaded.
/// </param>
public class OtpRepository(IdentityDbContext context, UserErrors userErrors, IPasswordService passwordService)
    : IOtpRepository
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
        if (passwordService.Verify(password: code, hash: candidateOtp.CodeHash))
        {
            return candidateOtp;
        }

        // Now increment the attempt count
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

        // Check if a consumed OTP exists and that it is the one the caller presented
        if (matchingOtp == null || !passwordService.Verify(password: code, hash: matchingOtp.CodeHash))
        {
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
    public async Task InvalidateExistingOtpsAsync(
        Guid userId,
        EnumOtpPurpose purpose,
        CancellationToken cancellationToken = default
    )
    {
        var specification = new OtpForInvalidationSpecification(userId: userId, purpose: purpose);
        List<OtpEntity> expiredOtpList = await context
            .Otps.ApplySpecification(specification: specification)
            .ToListAsync(cancellationToken: cancellationToken);

        foreach (OtpEntity otp in expiredOtpList)
        {
            otp.MarkAsUsed();
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
