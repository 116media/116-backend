using _116.Shared.Contracts.Application.CQRS;
using _116.User.Application.Shared.Repositories;
using _116.User.Application.Shared.Services;
using _116.User.Domain.Entities;
using _116.User.Domain.ValueObjects;
using OtpPurpose = _116.User.Domain.Enums.OtpPurpose;

namespace _116.User.Application.Admin.UseCases.Commands.ForgotPassword;

/// <summary>
/// Handles the <see cref="AdminForgotPasswordCommand"/> to initiate password reset for existing admin users.
/// </summary>
public class AdminForgotPasswordHandler(
    IUserRepository userRepository,
    IOtpRepository otpRepository,
    IOtpService otpService
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
            return new AdminForgotPasswordResult(IsSuccess: true);
        }

        UserEntity user = await userRepository.GetUserWithRolesOrThrowAsync(email, cancellationToken);

        userRepository.IsUserAccountActive(user);

        OtpEntity passwordResetOtp = otpService.CreateOtp(user.Id, OtpPurpose.PasswordReset);

        await otpRepository.AddAsync(passwordResetOtp, cancellationToken);
        await otpRepository.SaveChangesAsync(cancellationToken);

        return new AdminForgotPasswordResult(IsSuccess: true);
    }
}
