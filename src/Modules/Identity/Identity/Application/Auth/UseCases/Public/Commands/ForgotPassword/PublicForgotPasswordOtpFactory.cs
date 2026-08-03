using _116.Identity.Application.Auth.Repositories;
using _116.Identity.Application.Auth.Services;
using _116.Identity.Application.Auth.UseCases.Public.Commands.ForgotPassword.Contracts;
using _116.Identity.Application.Shared.Persistence;
using _116.Identity.Domain.Enums;

namespace _116.Identity.Application.Auth.UseCases.Public.Commands.ForgotPassword;

/// <summary>
/// Factory implementation for handling forgot password OTP creation and persistence.
/// </summary>
/// <param name="otpRepository">Repository for OTP data access operations.</param>
/// <param name="otpService">Service for OTP generation.</param>
/// <param name="unitOfWork">Unit of Work for managing database transactions.</param>
public class PublicForgotPasswordOtpFactory(
    IOtpRepository otpRepository,
    IOtpService otpService,
    IIdentityUnitOfWork unitOfWork
) : IPublicForgotPasswordOtpFactory
{
    /// <summary>
    /// Creates and persists an OTP for password reset.
    /// </summary>
    public async Task<OtpCreationResult> CreatePasswordResetOtpAsync(Guid userId, CancellationToken cancellationToken)
    {
        OtpCreationResult passwordResetOtp = otpService.CreateOtp(
            userId: userId,
            purpose: EnumOtpPurpose.PasswordReset
        );
        await otpRepository.AddAsync(otp: passwordResetOtp.Otp, cancellationToken: cancellationToken);
        await unitOfWork.CommitAsync(cancellationToken: cancellationToken);

        return passwordResetOtp;
    }
}
