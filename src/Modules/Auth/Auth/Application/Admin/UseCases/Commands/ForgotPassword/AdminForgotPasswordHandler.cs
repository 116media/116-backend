using _116.Auth.Application.Shared.Persistence;
using _116.Shared.Contracts.Application.CQRS;
using _116.Auth.Application.Shared.Repositories;
using _116.Auth.Application.Shared.Services;
using _116.Auth.Domain.Entities;
using _116.Auth.Domain.Enums;
using _116.Auth.Domain.ValueObjects;

namespace _116.Auth.Application.Admin.UseCases.Commands.ForgotPassword;

/// <summary>
/// Handles the <see cref="AdminForgotPasswordCommand"/> to initiate password reset for existing admin users.
/// </summary>
public class AdminForgotPasswordHandler(
    IUserRepository userRepository,
    IOtpRepository otpRepository,
    IOtpService otpService,
    IAuthUnitOfWork unitOfWork
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
        var email = new Email(command.Email);

        if (!await userRepository.ExistsByEmailAsync(email, cancellationToken))
        {
            return new AdminForgotPasswordResult(IsSuccess: true, Email: command.Email);
        }

        UserEntity? user = await userRepository.GetUserWithRolesByEmailOrThrow(email, cancellationToken);

        // Validate admin account status
        userRepository.IsUserAdmin(user!);
        userRepository.IsUserAccountActive(user!);

        OtpEntity passwordResetOtp = otpService.CreateOtp(user!.Id, EnumOtpPurpose.PasswordReset);

        await otpRepository.AddAsync(passwordResetOtp, cancellationToken);
        await unitOfWork.CommitAsync(cancellationToken);

        return new AdminForgotPasswordResult(IsSuccess: true, Email: command.Email);
    }
}
