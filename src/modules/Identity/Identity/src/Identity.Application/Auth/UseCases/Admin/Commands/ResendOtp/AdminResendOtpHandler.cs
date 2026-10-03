using _116.BuildingBlocks.Application.CQRS;
using _116.BuildingBlocks.Presentation.Constants;
using _116.Identity.Application.Auth.UseCases.Admin.Commands.ResendOtp.Contracts;
using _116.Identity.Application.Shared.Repositories;
using _116.Identity.Domain.Entities;
using _116.Identity.Domain.Enums;
using _116.Identity.Domain.ValueObjects;

namespace _116.Identity.Application.Auth.UseCases.Admin.Commands.ResendOtp;

/// <summary>
/// Handles the <see cref="AdminResendOtpCommand" /> to resend OTP codes for admin users.
/// </summary>
/// <param name="otpService">Service for handling admin OTP resend logic.</param>
/// <param name="authRepository">Repository for user data access operations.</param>
public class AdminResendOtpHandler(IAdminResendOtpService otpService, IAuthRepository authRepository)
    : ICommandHandler<AdminResendOtpCommand, AdminResendOtpResult>
{
    /// <summary>
    /// Handles the resend OTP command by invalidating existing OTPs and generating a new one.
    /// </summary>
    /// <param name="command">The resend OTP command containing email and purpose.</param>
    /// <param name="cancellationToken">Token to observe for cancellation requests.</param>
    /// <returns>The result indicating success or failure of the OTP resend operation.</returns>
    /// <exception cref="NotFoundException">Thrown when the admin user is not found.</exception>
    /// <exception cref="BadRequestException">Thrown when the admin account is inactive or not verified.</exception>
    public async Task<AdminResendOtpResult> Handle(AdminResendOtpCommand command, CancellationToken cancellationToken)
    {
        var email = new Email(value: command.Email);
        var purpose = new OtpPurpose(value: command.Purpose);
        if (!await authRepository.ExistsByEmailAsync(email: email, cancellationToken: cancellationToken))
        {
            return new AdminResendOtpResult(IsSuccess: true);
        }

        UserEntity? user = await authRepository.GetUserWithRolesByEmailOrThrow(
            email: email,
            cancellationToken: cancellationToken
        );

        authRepository.IsUserAdmin(user!);
        authRepository.IsUserAccountActive(user!);

        await otpService.ResendOtpAsync(userId: user!.Id, purpose: purpose, cancellationToken: cancellationToken);

        return new AdminResendOtpResult(IsSuccess: true);
    }
}
