using _116.BuildingBlocks.Constants;
using _116.Identity.Application.Auth.UseCases.Admin.Commands.ForgotPassword.Contracts;
using _116.Identity.Application.Shared.Repositories;
using _116.Identity.Domain.Entities;
using _116.Identity.Domain.ValueObjects;
using _116.Mailer.Contracts.Application;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Identity.Application.Auth.UseCases.Admin.Commands.ForgotPassword;

/// <summary>
/// Handles the <see cref="AdminForgotPasswordCommand" /> to initiate password reset for existing admin users.
/// </summary>
/// <param name="otpFactory">Factory for handling admin forgot password OTP creation.</param>
/// <param name="authRepository">Repository for user data access operations.</param>
/// <param name="mailer">Outbox mailer delivering the reset code.</param>
public class AdminForgotPasswordHandler(
    IAdminForgotPasswordOtpFactory otpFactory,
    IAuthRepository authRepository,
    IMailer mailer
) : ICommandHandler<AdminForgotPasswordCommand, AdminForgotPasswordResult>
{
    /// <summary>
    /// Handles the forgot password command by generating an OTP for password reset.
    /// Always returns success to prevent user enumeration attacks.
    /// </summary>
    public async Task<AdminForgotPasswordResult> Handle(
        AdminForgotPasswordCommand command,
        CancellationToken cancellationToken
    )
    {
        var email = new Email(value: command.Email);
        if (!await authRepository.ExistsByEmailAsync(email: email, cancellationToken: cancellationToken))
        {
            return new AdminForgotPasswordResult(IsSuccess: true, Email: command.Email);
        }

        UserEntity? user = await authRepository.GetUserWithRolesByEmailOrThrow(
            email: email,
            cancellationToken: cancellationToken
        );

        authRepository.IsUserAdmin(user!);
        authRepository.IsUserAccountActive(user!);

        OtpEntity passwordResetOtp = await otpFactory.CreatePasswordResetOtpAsync(
            userId: user!.Id,
            cancellationToken: cancellationToken
        );

        if (user.Email is not null)
        {
            await mailer.EnqueueAsync(
                template: EnumEmailTemplate.PasswordResetOtp,
                to: new EmailRecipient(Address: user.Email, DisplayName: user.UserName),
                tokens: new Dictionary<string, string>
                {
                    ["userName"] = user.UserName,
                    ["otpCode"] = passwordResetOtp.Code,
                    ["expiryMinutes"] = UserConstants.OtpExpirationMinutes.ToString(),
                },
                culture: EmailCulture.Current(),
                cancellationToken: cancellationToken
            );
        }

        return new AdminForgotPasswordResult(IsSuccess: true, Email: command.Email);
    }
}
