using _116.BuildingBlocks.Constants;
using _116.Identity.Application.Auth.Services;
using _116.Identity.Application.Auth.UseCases.Public.Commands.ForgotPassword.Contracts;
using _116.Identity.Application.Shared.Repositories;
using _116.Identity.Domain.Entities;
using _116.Identity.Domain.ValueObjects;
using _116.Shared.Contracts.Application.CQRS;
using Microsoft.Extensions.Logging;

namespace _116.Identity.Application.Auth.UseCases.Public.Commands.ForgotPassword;

/// <summary>
/// Handles the <see cref="PublicForgotPasswordCommand" /> to initiate password reset for existing users.
/// </summary>
/// <param name="otpFactory">Factory for handling forgot password OTP creation.</param>
/// <param name="authRepository">Repository for user data access operations.</param>
/// <param name="logger">Logger recording why a request was refused, since the caller is not told.</param>
public class PublicForgotPasswordHandler(
    IPublicForgotPasswordOtpFactory otpFactory,
    IAuthRepository authRepository,
    ILogger<PublicForgotPasswordHandler> logger
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
            return new PublicForgotPasswordResult(IsSuccess: true, Email: command.Email);
        }

        UserEntity? user = await authRepository.GetUserWithRolesByEmailOrThrow(
            email: email,
            cancellationToken: cancellationToken
        );

        // Answer identically whether the address is unknown, inactive or unverified.
        if (!user!.IsActive || !user.IsVerified)
        {
            logger.LogInformation(
                "Forgot-password refused for an ineligible account; answering with the neutral result."
            );
            return new PublicForgotPasswordResult(IsSuccess: true, Email: command.Email);
        }

        await otpFactory.CreatePasswordResetOtpAsync(userId: user!.Id, cancellationToken: cancellationToken);

        return new PublicForgotPasswordResult(IsSuccess: true, Email: command.Email);
    }
}
