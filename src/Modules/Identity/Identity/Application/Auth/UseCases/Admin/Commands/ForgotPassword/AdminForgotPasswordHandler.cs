using _116.BuildingBlocks.Constants;
using _116.Identity.Application.Auth.Services;
using _116.Identity.Application.Auth.UseCases.Admin.Commands.ForgotPassword.Contracts;
using _116.Identity.Application.Roles.Specifications;
using _116.Identity.Application.Shared.Repositories;
using _116.Identity.Domain.Entities;
using _116.Identity.Domain.ValueObjects;
using _116.Mailer.Contracts.Application;
using _116.Shared.Contracts.Application.CQRS;
using Microsoft.Extensions.Logging;

namespace _116.Identity.Application.Auth.UseCases.Admin.Commands.ForgotPassword;

/// <summary>
/// Handles the <see cref="AdminForgotPasswordCommand" /> to initiate password reset for existing admin users.
/// </summary>
/// <param name="otpFactory">Factory for handling admin forgot password OTP creation.</param>
/// <param name="authRepository">Repository for user data access operations.</param>
/// <param name="mailer">Outbox mailer delivering the reset code.</param>
/// <param name="logger">Logger recording why a request was refused, since the caller is not told.</param>
public class AdminForgotPasswordHandler(
    IAdminForgotPasswordOtpFactory otpFactory,
    IAuthRepository authRepository,
    IMailer mailer,
    ILogger<AdminForgotPasswordHandler> logger
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

        // Answer identically whether the address is unknown, not an administrator, or inactive.
        // Anything that changes the response here identifies privileged accounts.
        bool isEligible = new UserHasAdminRoleSpecification().IsSatisfiedBy(entity: user!) && user!.IsActive;
        if (!isEligible)
        {
            logger.LogInformation(
                "Admin forgot-password refused for an ineligible account; answering with the neutral result."
            );
            return new AdminForgotPasswordResult(IsSuccess: true, Email: command.Email);
        }

        OtpCreationResult passwordResetOtp = await otpFactory.CreatePasswordResetOtpAsync(
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
                    ["otpCode"] = passwordResetOtp.PlainCode,
                    ["expiryMinutes"] = UserConstants.OtpExpirationMinutes.ToString(),
                },
                culture: EmailCulture.Current(),
                cancellationToken: cancellationToken
            );
        }

        return new AdminForgotPasswordResult(IsSuccess: true, Email: command.Email);
    }
}
