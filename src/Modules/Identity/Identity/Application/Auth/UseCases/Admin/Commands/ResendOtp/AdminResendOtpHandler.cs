using _116.Identity.Application.Auth.Repositories;
using _116.Identity.Application.Auth.Services;
using _116.Identity.Application.Shared.Persistence;
using _116.Identity.Application.Shared.Repositories;
using _116.Identity.Domain.Entities;
using _116.Identity.Domain.ValueObjects;
using _116.Shared.Application.Exceptions;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Identity.Application.Auth.UseCases.Admin.Commands.ResendOtp;

/// <summary>
/// Handles the <see cref="AdminResendOtpCommand" /> to resend OTP codes for admin users.
/// </summary>
public class AdminResendOtpHandler(
    IAuthRepository authRepository,
    IOtpRepository otpRepository,
    IOtpService otpService,
    IIdentityUnitOfWork unitOfWork
) : ICommandHandler<AdminResendOtpCommand, AdminResendOtpResult>
{
    /// <summary>
    /// Handles the resend OTP command by invalidating existing OTPs and generating a new one.
    /// </summary>
    /// <param name="command">The resend OTP command containing email and purpose.</param>
    /// <param name="cancellationToken">Token to observe for cancellation requests.</param>
    /// <returns>The result indicating success or failure of the OTP resend operation.</returns>
    /// <exception cref="NotFoundException">Thrown when the admin user is not found.</exception>
    /// <exception cref="BadRequestException">Thrown when the admin account is inactive or not verified.</exception>
    public async Task<AdminResendOtpResult> Handle(
        AdminResendOtpCommand command,
        CancellationToken cancellationToken
    )
    {
        var email = new Email(value: command.Email);
        var purpose = new OtpPurpose(value: command.Purpose);
        if (!await authRepository.ExistsByEmailAsync(email: email, cancellationToken: cancellationToken))
        {
            return new AdminResendOtpResult(true);
        }

        UserEntity? user =
            await authRepository.GetUserWithRolesByEmailOrThrow(email: email, cancellationToken: cancellationToken);
        // Validate admin account status
        authRepository.IsUserAdmin(user!);
        authRepository.IsUserAccountActive(user!);
        // Invalidate existing OTPs for this purpose
        await otpRepository.InvalidateExistingOtpsAsync(userId: user!.Id, purpose: purpose,
            cancellationToken: cancellationToken);
        // Create new OTP
        OtpEntity newOtp = otpService.CreateOtp(userId: user.Id, purpose: purpose);
        // Save the new OTP
        await otpRepository.AddAsync(otp: newOtp, cancellationToken: cancellationToken);
        await unitOfWork.CommitAsync(cancellationToken: cancellationToken);
        return new AdminResendOtpResult(true);
    }
}
