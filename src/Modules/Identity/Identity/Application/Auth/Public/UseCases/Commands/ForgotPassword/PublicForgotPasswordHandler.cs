using _116.Identity.Application.Shared.Persistence;
using _116.Identity.Application.Shared.Repositories;
using _116.Identity.Application.Shared.Services;
using _116.Identity.Domain.Entities;
using _116.Identity.Domain.Enums;
using _116.Identity.Domain.ValueObjects;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Identity.Application.Auth.Public.UseCases.Commands.ForgotPassword;

/// <summary>
/// Handles the <see cref="PublicForgotPasswordCommand"/> to initiate password reset for existing users.
/// </summary>
public class PublicForgotPasswordHandler(
    IUserRepository userRepository,
    IOtpRepository otpRepository,
    IOtpService otpService,
    IIdentityUnitOfWork unitOfWork
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
        UserEntity? user = await userRepository.GetUserWithRolesByEmailOrThrow(email, cancellationToken);
        // Validate user account status - must be active and verified
        userRepository.IsUserAccountActive(user!);
        userRepository.IsUserAccountVerified(user!);
        OtpEntity passwordResetOtp = otpService.CreateOtp(user!.Id, EnumOtpPurpose.PasswordReset);
        await otpRepository.AddAsync(passwordResetOtp, cancellationToken);
        await unitOfWork.CommitAsync(cancellationToken);
        return new PublicForgotPasswordResult(IsSuccess: true, Email: command.Email);
    }
}
