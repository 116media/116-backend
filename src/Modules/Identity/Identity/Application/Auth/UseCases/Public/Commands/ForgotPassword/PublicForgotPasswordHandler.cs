using _116.Identity.Application.Auth.Repositories;
using _116.Identity.Application.Auth.Services;
using _116.Identity.Application.Shared.Persistence;
using _116.Identity.Application.Shared.Repositories;
using _116.Identity.Domain.Entities;
using _116.Identity.Domain.Enums;
using _116.Identity.Domain.ValueObjects;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Identity.Application.Auth.UseCases.Public.Commands.ForgotPassword;

/// <summary>
/// Handles the <see cref="PublicForgotPasswordCommand" /> to initiate password reset for existing users.
/// </summary>
public class PublicForgotPasswordHandler(
    IAuthRepository authRepository,
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
        var email = new Email(value: command.Email);
        if (!await authRepository.ExistsByEmailAsync(email: email, cancellationToken: cancellationToken))
        {
            return new PublicForgotPasswordResult(true, Email: command.Email);
        }

        UserEntity? user =
            await authRepository.GetUserWithRolesByEmailOrThrow(email: email, cancellationToken: cancellationToken);
        // Validate user account status - must be active and verified
        authRepository.IsUserAccountActive(user!);
        authRepository.IsUserAccountVerified(user!);
        OtpEntity passwordResetOtp = otpService.CreateOtp(userId: user!.Id, purpose: EnumOtpPurpose.PasswordReset);
        await otpRepository.AddAsync(otp: passwordResetOtp, cancellationToken: cancellationToken);
        await unitOfWork.CommitAsync(cancellationToken: cancellationToken);
        return new PublicForgotPasswordResult(true, Email: command.Email);
    }
}
