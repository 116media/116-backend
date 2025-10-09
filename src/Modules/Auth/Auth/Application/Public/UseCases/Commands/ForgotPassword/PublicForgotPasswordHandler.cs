using _116.Shared.Contracts.Application.CQRS;
using _116.Auth.Application.Shared.Repositories;
using _116.Auth.Application.Shared.Services;
using _116.Auth.Domain.Entities;
using _116.Auth.Domain.ValueObjects;
using OtpPurpose = _116.Auth.Domain.Enums.OtpPurpose;

namespace _116.Auth.Application.Public.UseCases.Commands.ForgotPassword;

/// <summary>
/// Handles the <see cref="PublicForgotPasswordCommand"/> to initiate password reset for existing users.
/// </summary>
public class PublicForgotPasswordHandler(
    IUserRepository userRepository,
    IOtpRepository otpRepository,
    IOtpService otpService
) : ICommandHandler<PublicForgotPasswordCommand, PublicForgotPasswordResult>
{
    /// <summary>
    /// Handles the forgot password command by generating an OTP for password reset.
    /// Always returns success to prevent user enumeration attacks.
    /// </summary>
    public async Task<PublicForgotPasswordResult> Handle(
        PublicForgotPasswordCommand command,
        CancellationToken cancellationToken
    )
    {
        var email = new Email(command.Email);

        if (!await userRepository.ExistsByEmailAsync(email, cancellationToken))
        {
            return new PublicForgotPasswordResult(IsSuccess: true, Email: command.Email);
        }

        UserEntity user = await userRepository.GetUserWithRolesOrThrowAsync(email, cancellationToken);

        userRepository.IsUserAccountActive(user);
        userRepository.IsUserAccountVerified(user);

        OtpEntity passwordResetOtp = otpService.CreateOtp(user.Id, OtpPurpose.PasswordReset);

        await otpRepository.AddAsync(passwordResetOtp, cancellationToken);
        await otpRepository.SaveChangesAsync(cancellationToken);

        return new PublicForgotPasswordResult(IsSuccess: true, Email: command.Email);
    }
}
