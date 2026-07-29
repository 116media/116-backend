using _116.BuildingBlocks.Constants;
using _116.Identity.Application.Auth.UseCases.Public.Commands.ResendOtp.Contracts;
using _116.Identity.Application.Shared.Repositories;
using _116.Identity.Domain.Entities;
using _116.Identity.Domain.Enums;
using _116.Identity.Domain.ValueObjects;
using _116.Mailer.Contracts.Application;
using _116.Shared.Application.Exceptions;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Identity.Application.Auth.UseCases.Public.Commands.ResendOtp;

/// <summary>
/// Handles the <see cref="PublicResendOtpCommand" /> to resend OTP codes for public users.
/// </summary>
/// <param name="otpFactory">Factory for handling OTP resend logic.</param>
/// <param name="authRepository">Repository for user data access operations.</param>
/// <param name="mailer">Outbox mailer re-delivering the code.</param>
public class PublicResendOtpHandler(IPublicResendOtpFactory otpFactory, IAuthRepository authRepository, IMailer mailer)
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
            return new PublicResendOtpResult(IsSuccess: true);
        }

        UserEntity? user = await authRepository.GetUserWithRolesByEmailOrThrow(
            email: email,
            cancellationToken: cancellationToken
        );
        authRepository.IsUserAccountActive(user!);

        OtpEntity resentOtp = await otpFactory.ResendOtpAsync(
            userId: user!.Id,
            purpose: purpose,
            cancellationToken: cancellationToken
        );

        EnumEmailTemplate? template = purpose.Value switch
        {
            EnumOtpPurpose.EmailVerification => EnumEmailTemplate.EmailVerificationOtp,
            EnumOtpPurpose.PasswordReset => EnumEmailTemplate.PasswordResetOtp,
            // TwoFactorAuthentication and AccountRecovery have no live flow and
            // therefore no template; the OTP row still rotates.
            _ => null,
        };

        if (template is not null && user.Email is not null)
        {
            await mailer.EnqueueAsync(
                template: template.Value,
                to: new EmailRecipient(Address: user.Email, DisplayName: user.UserName),
                tokens: new Dictionary<string, string>
                {
                    ["userName"] = user.UserName,
                    ["otpCode"] = resentOtp.Code,
                    ["expiryMinutes"] = UserConstants.OtpExpirationMinutes.ToString(),
                },
                culture: EmailCulture.Current(),
                cancellationToken: cancellationToken
            );
        }

        return new PublicResendOtpResult(IsSuccess: true);
    }
}
