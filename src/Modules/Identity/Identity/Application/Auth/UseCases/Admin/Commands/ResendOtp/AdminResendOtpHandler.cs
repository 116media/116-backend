using _116.Identity.Application.Auth.UseCases.Admin.Commands.ResendOtp.Contracts;
using _116.Identity.Application.Shared.Repositories;
using _116.Identity.Domain.Entities;
using _116.Identity.Domain.ValueObjects;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Identity.Application.Auth.UseCases.Admin.Commands.ResendOtp;

/// <summary>
/// Handles the <see cref="AdminResendOtpCommand" /> to resend OTP codes for admin users.
/// </summary>
/// <param name="otpFactory">Factory for handling admin OTP resend logic.</param>
/// <param name="authRepository">Repository for user data access operations.</param>
public class AdminResendOtpHandler(IAdminResendOtpFactory otpFactory, IAuthRepository authRepository)
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
            return new AdminResendOtpResult(true);
        }

        UserEntity? user = await authRepository.GetUserWithRolesByEmailOrThrow(
            email: email,
            cancellationToken: cancellationToken
        );

        authRepository.IsUserAdmin(user!);
        authRepository.IsUserAccountActive(user!);

        await otpFactory.ResendOtpAsync(userId: user!.Id, purpose: purpose, cancellationToken: cancellationToken);

        return new AdminResendOtpResult(true);
    }
}
