using _116.Identity.Application.Auth.UseCases.Public.Commands.ResendOtp.Contracts;
using _116.Identity.Application.Shared.Repositories;
using _116.Identity.Domain.Entities;
using _116.Identity.Domain.ValueObjects;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Identity.Application.Auth.UseCases.Public.Commands.ResendOtp;

/// <summary>
/// Handles the <see cref="PublicResendOtpCommand" /> to resend OTP codes for public users.
/// </summary>
/// <param name="otpFactory">Factory for handling OTP resend logic.</param>
/// <param name="authRepository">Repository for user data access operations.</param>
public class PublicResendOtpHandler(IPublicResendOtpFactory otpFactory, IAuthRepository authRepository)
    : ICommandHandler<PublicResendOtpCommand, PublicResendOtpResult>
{
    /// <summary>
    /// Handles the resend OTP command by invalidating existing OTPs and generating a new one.
    /// </summary>
    /// <param name="command">The resend OTP command containing email and purpose.</param>
    /// <param name="cancellationToken">Token to observe for cancellation requests.</param>
    /// <returns>The result indicating success or failure of the OTP resend operation.</returns>
    /// <exception cref="NotFoundException">Thrown when the user is not found.</exception>
    /// <exception cref="BadRequestException">Thrown when the user account is inactive or not verified.</exception>
    public async Task<PublicResendOtpResult> Handle(PublicResendOtpCommand command, CancellationToken cancellationToken)
    {
        var email = new Email(value: command.Email);
        var purpose = new OtpPurpose(value: command.Purpose);
        if (!await authRepository.ExistsByEmailAsync(email: email, cancellationToken: cancellationToken))
        {
            return new PublicResendOtpResult(true);
        }

        UserEntity? user = await authRepository.GetUserWithRolesByEmailOrThrow(
            email: email,
            cancellationToken: cancellationToken
        );
        authRepository.IsUserAccountActive(user!);

        await otpFactory.ResendOtpAsync(userId: user!.Id, purpose: purpose, cancellationToken: cancellationToken);

        return new PublicResendOtpResult(true);
    }
}
