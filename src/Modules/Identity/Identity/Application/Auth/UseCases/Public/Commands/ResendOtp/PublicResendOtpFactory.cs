using _116.BuildingBlocks.Constants;
using _116.Identity.Application.Auth.Repositories;
using _116.Identity.Application.Auth.Services;
using _116.Identity.Application.Auth.UseCases.Public.Commands.ResendOtp.Contracts;
using _116.Identity.Application.Shared.Persistence;
using _116.Identity.Domain.ValueObjects;

namespace _116.Identity.Application.Auth.UseCases.Public.Commands.ResendOtp;

/// <summary>
/// Factory implementation for handling OTP resend logic.
/// </summary>
/// <param name="otpRepository">Repository for OTP data access operations.</param>
/// <param name="otpService">Service for OTP generation.</param>
/// <param name="unitOfWork">Unit of Work for managing database transactions.</param>
public class PublicResendOtpFactory(
    IOtpRepository otpRepository,
    IOtpService otpService,
    IIdentityUnitOfWork unitOfWork
) : IPublicResendOtpFactory
{
    /// <summary>
    /// Invalidates existing OTPs and creates a new OTP for the specified purpose.
    /// </summary>
    public async Task<OtpCreationResult?> ResendOtpAsync(
        Guid userId,
        OtpPurpose purpose,
        CancellationToken cancellationToken
    )
    {
        int issuedInWindow = await otpRepository.CountRecentOtpsAsync(
            userId: userId,
            purpose: purpose,
            cancellationToken: cancellationToken
        );

        // Over the cap the caller still gets the neutral success, so the refusal cannot be used
        // to tell an existing account from a missing one.
        if (issuedInWindow >= UserConstants.MaxOtpResendsPerWindow)
        {
            return null;
        }

        await otpRepository.InvalidateExistingOtpsAsync(
            userId: userId,
            purpose: purpose,
            cancellationToken: cancellationToken
        );

        OtpCreationResult newOtp = otpService.CreateOtp(userId: userId, purpose: purpose);

        await otpRepository.AddAsync(otp: newOtp.Otp, cancellationToken: cancellationToken);
        await unitOfWork.CommitAsync(cancellationToken: cancellationToken);

        return newOtp;
    }
}
