using _116.Identity.Application.Auth.UseCases.Admin.Commands.ForgotPassword.Contracts;
using _116.Identity.Application.Shared.Repositories;
using _116.Identity.Domain.Entities;
using _116.Identity.Domain.ValueObjects;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Identity.Application.Auth.UseCases.Admin.Commands.ForgotPassword;

/// <summary>
/// Handles the <see cref="AdminForgotPasswordCommand" /> to initiate password reset for existing admin users.
/// </summary>
/// <param name="otpFactory">Factory for handling admin forgot password OTP creation.</param>
/// <param name="authRepository">Repository for user data access operations.</param>
public class AdminForgotPasswordHandler(IAdminForgotPasswordOtpFactory otpFactory, IAuthRepository authRepository)
    : ICommandHandler<AdminForgotPasswordCommand, AdminForgotPasswordResult>
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
            return new AdminForgotPasswordResult(true, Email: command.Email);
        }

        UserEntity? user = await authRepository.GetUserWithRolesByEmailOrThrow(
            email: email,
            cancellationToken: cancellationToken
        );

        authRepository.IsUserAdmin(user!);
        authRepository.IsUserAccountActive(user!);

        await otpFactory.CreatePasswordResetOtpAsync(userId: user!.Id, cancellationToken: cancellationToken);

        return new AdminForgotPasswordResult(true, Email: command.Email);
    }
}
