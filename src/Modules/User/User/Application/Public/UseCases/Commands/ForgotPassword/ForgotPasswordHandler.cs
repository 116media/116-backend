using _116.Shared.Contracts.Application.CQRS;
using _116.User.Application.Shared.Repositories;
using _116.User.Application.Shared.Services;
using _116.User.Domain.Entities;
using _116.User.Domain.Enums;
using _116.User.Domain.ValueObjects;

namespace _116.User.Application.Public.UseCases.Commands.ForgotPassword;

/// <summary>
/// Handles the <see cref="ForgotPasswordCommand"/> to initiate password reset for existing users.
/// </summary>
public class ForgotPasswordHandler(
    IUserRepository userRepository,
    IOtpRepository otpRepository,
    IOtpService otpService
) : ICommandHandler<ForgotPasswordCommand, ForgotPasswordResult>
{
    /// <summary>
    /// Handles the forgot password command by generating an OTP for password reset.
    /// Always returns success to prevent user enumeration attacks.
    /// </summary>
    public async Task<ForgotPasswordResult> Handle(ForgotPasswordCommand command, CancellationToken cancellationToken)
    {
        var email = new Email(command.Email);

        if (!await userRepository.ExistsByEmailAsync(email, cancellationToken))
        {
            return new ForgotPasswordResult(IsSuccess: true);
        }

        UserEntity user = await userRepository.GetUserWithRolesOrThrowAsync(email, cancellationToken);

        userRepository.IsUserAccountActive(user);
        userRepository.IsUserAccountVerified(user);

        OtpEntity passwordResetOtp = otpService.CreateOtp(user.Id, OtpPurpose.PasswordReset);

        await otpRepository.AddAsync(passwordResetOtp, cancellationToken);
        await otpRepository.SaveChangesAsync(cancellationToken);

        return new ForgotPasswordResult(IsSuccess: true);
    }
}
