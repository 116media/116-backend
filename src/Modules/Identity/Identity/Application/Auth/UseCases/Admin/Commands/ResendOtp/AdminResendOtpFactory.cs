using _116.Identity.Application.Auth.Repositories;
using _116.Identity.Application.Auth.Services;
using _116.Identity.Application.Auth.UseCases.Admin.Commands.ResendOtp.Contracts;
using _116.Identity.Application.Shared.Persistence;
using _116.Identity.Domain.Entities;
using _116.Identity.Domain.ValueObjects;

namespace _116.Identity.Application.Auth.UseCases.Admin.Commands.ResendOtp;

/// <summary>
/// Factory implementation for handling admin OTP resend logic.
/// </summary>
/// <param name="otpRepository">Repository for OTP data access operations.</param>
/// <param name="otpService">Service for OTP generation.</param>
/// <param name="unitOfWork">Unit of Work for managing database transactions.</param>
public class AdminResendOtpFactory(
    IOtpRepository otpRepository,
    IOtpService otpService,
    IIdentityUnitOfWork unitOfWork
) : IAdminResendOtpFactory
{
    /// <summary>
    /// Invalidates existing OTPs and creates a new OTP for the specified purpose.
    /// </summary>
    public async Task<OtpEntity> ResendOtpAsync(
        Guid userId,
        OtpPurpose purpose,
        CancellationToken cancellationToken
    )
    {
        await otpRepository.InvalidateExistingOtpsAsync(userId: userId, purpose: purpose,
            cancellationToken: cancellationToken);

        OtpEntity newOtp = otpService.CreateOtp(userId: userId, purpose: purpose);

        await otpRepository.AddAsync(otp: newOtp, cancellationToken: cancellationToken);
        await unitOfWork.CommitAsync(cancellationToken: cancellationToken);

        return newOtp;
    }
}
