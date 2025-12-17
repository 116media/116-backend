using _116.Identity.Application.Shared.Persistence;
using _116.Identity.Application.Shared.Repositories;
using _116.Identity.Application.Shared.Services;
using _116.Identity.Domain.Entities;
using _116.Identity.Domain.Enums;
using _116.Identity.Domain.ValueObjects;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Identity.Application.Auth.Admin.UseCases.Commands.ForgotPassword;

/// <summary>
/// Handles the <see cref="AdminForgotPasswordCommand"/> to initiate password reset for existing admin users.
/// </summary>
public class AdminForgotPasswordHandler(
    IUserRepository userRepository,
    IOtpRepository otpRepository,
    IOtpService otpService,
    IIdentityUnitOfWork unitOfWork
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
