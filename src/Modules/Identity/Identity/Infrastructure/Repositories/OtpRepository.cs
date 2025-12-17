using _116.Shared.Infrastructure.Extensions;
using _116.Identity.Application.Shared.Errors;
using _116.Identity.Application.Shared.Repositories;
using _116.Identity.Application.Shared.Specifications;
using _116.Identity.Domain.Entities;
using _116.Identity.Domain.Enums;
using _116.Identity.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace _116.Identity.Infrastructure.Repositories;

/// <summary>
/// Implementation of <see cref="IOtpRepository"/> using Entity Framework Core.
/// </summary>
public class OtpRepository(IdentityDbContext context) : IOtpRepository
{
    /// <inheritdoc />
    public async Task AddAsync(OtpEntity otp, CancellationToken cancellationToken = default)
    {
        await context.Otps.AddAsync(otp, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<OtpEntity?> GetLatestValidOtpAsync(
        Guid userId,
        EnumOtpPurpose purpose,
        CancellationToken cancellationToken = default
    )
    {
        var specification = new OtpIsValidForUserAndPurposeSpecification(userId, purpose);

        return await context.Otps
            .ApplySpecification(specification)
            .OrderByDescending(o => o.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<OtpEntity> ValidateOtpAsync(
        Guid userId,
        string code,
        EnumOtpPurpose purpose,
        CancellationToken cancellationToken = default
    )
    {
        // Use specification to find OTP with the provided code
        var specification = new OtpForValidationSpecification(userId, code, purpose);

        OtpEntity? matchingOtp = await context.Otps
            .ApplySpecification(specification)
            .OrderByDescending(o => o.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);

        if (matchingOtp != null)
        {
            // First check if the otp is not expired
            if (matchingOtp.IsExpired())
            {
                throw UserErrors.OtpExpired();
            }

            // Then check if the max attempts are not reached
            if (matchingOtp.HasMaxAttemptsReached())
            {
                throw UserErrors.MaxOtpAttemptsReached();
            }

            // Then check if the otp is valid for the user
            if (matchingOtp.IsValid())
            {
                return matchingOtp;
            }

            // Now increment the attempt count
            matchingOtp.IncrementAttemptCount();

            await UpdateAsync(matchingOtp, cancellationToken);
            await SaveChangesAsync(cancellationToken);

            if (matchingOtp.HasMaxAttemptsReached()) throw UserErrors.MaxOtpAttemptsReached();

            throw UserErrors.InvalidOtpCode();
        }

        // No matching OTP found — check the latest valid OTP for this purpose
        OtpEntity? latestOtp = await GetLatestValidOtpAsync(userId, purpose, cancellationToken);

        if (latestOtp == null)
        {
            throw UserErrors.NoValidOtpFound();
        }

        if (latestOtp.HasMaxAttemptsReached())
        {
            throw UserErrors.MaxOtpAttemptsReached();
        }

        if (latestOtp.IsExpired())
        {
            throw UserErrors.OtpExpired();
        }

        // Increment attempts for wrong code
        latestOtp.IncrementAttemptCount();
        await UpdateAsync(latestOtp, cancellationToken);
        await SaveChangesAsync(cancellationToken);

        if (latestOtp.HasMaxAttemptsReached())
        {
            throw UserErrors.MaxOtpAttemptsReached();
        }

        throw UserErrors.InvalidOtpCode();
    }


    /// <inheritdoc />
    public async Task<OtpEntity> ValidateUsedOtpAsync(
        Guid userId,
        string code,
        EnumOtpPurpose purpose,
        CancellationToken cancellationToken = default
    )
    {
        // Use specification to find used OTP with the provided code
        var specification = new OtpForUsedValidationSpecification(userId, code, purpose);

        OtpEntity? matchingOtp = await context.Otps
            .ApplySpecification(specification)
            .OrderByDescending(o => o.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);

        // Check if OTP exists
        if (matchingOtp == null)
        {
            throw UserErrors.OtpNotYetVerified();
        }

        // Check if the OTP has expired
        if (matchingOtp.IsExpired())
        {
            throw UserErrors.OtpExpired();
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
        var specification = new OtpForInvalidationSpecification(userId, purpose);

        List<OtpEntity> expiredOtpList = await context.Otps
            .ApplySpecification(specification)
            .ToListAsync(cancellationToken);

        foreach (OtpEntity otp in expiredOtpList)
        {
            otp.MarkAsUsed();
        }
    }

    /// <inheritdoc />
    public async Task<int> CleanupExpiredOtpsAsync(CancellationToken cancellationToken = default)
    {
        var specification = new OtpIsExpiredSpecification();

        List<OtpEntity> expiredOtpList = await context.Otps
            .ApplySpecification(specification)
            .ToListAsync(cancellationToken);

        context.Otps.RemoveRange(expiredOtpList);
        return expiredOtpList.Count;
    }

    /// <inheritdoc />
    public Task UpdateAsync(OtpEntity otp, CancellationToken cancellationToken = default)
    {
        context.Otps.Update(otp);
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await context.SaveChangesAsync(cancellationToken);
    }
}
