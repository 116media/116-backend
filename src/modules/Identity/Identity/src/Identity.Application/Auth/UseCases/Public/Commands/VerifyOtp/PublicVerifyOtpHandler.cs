using _116.BuildingBlocks.Application.CQRS;
using _116.Identity.Application.Auth.Services;
using _116.Identity.Application.Shared.Errors.Facade;
using _116.Identity.Application.Shared.Persistence;
using _116.Identity.Application.Shared.Repositories;
using _116.Identity.Domain.Entities;
using _116.Identity.Domain.Enums;
using _116.Identity.Domain.ValueObjects;

namespace _116.Identity.Application.Auth.UseCases.Public.Commands.VerifyOtp;

/// <summary>
/// Handles the <see cref="PublicVerifyOtpCommand" /> to verify OTP codes for user account verification.
/// The welcome email reacts to the domain event the user aggregate raises when it transitions to
/// verified.
/// </summary>
/// <param name="authRepository">Repository for user data access operations.</param>
/// <param name="otpVerificationService">Service consuming the outstanding OTP.</param>
/// <param name="unitOfWork">Unit of Work for managing database transactions.</param>
/// <param name="i18n">Single i18n entry point for the Identity module.</param>
public class PublicVerifyOtpHandler(
    IAuthRepository authRepository,
    IOtpVerificationService otpVerificationService,
    IIdentityUnitOfWork unitOfWork,
    IdentityI18n i18n
) : ICommandHandler<PublicVerifyOtpCommand, PublicVerifyOtpResult>
{
    /// <inheritdoc />
    public async Task<PublicVerifyOtpResult> Handle(PublicVerifyOtpCommand command, CancellationToken cancellationToken)
    {
        var email = new Email(value: command.Email);
        var purpose = new OtpPurpose(value: command.Purpose);
        UserEntity? user = await authRepository.GetUserWithRolesByEmailOrThrow(
            email: email,
            cancellationToken: cancellationToken
        );

        // Only the email-verification purpose has an already-verified state to refuse.
        if (user!.IsVerified && purpose.Value == EnumOtpPurpose.EmailVerification)
        {
            throw i18n.User.AccountAlreadyVerified();
        }

        await otpVerificationService.ConsumeOtpAsync(
            userId: user.Id,
            purpose: purpose,
            code: command.Code,
            cancellationToken: cancellationToken
        );

        user.MarkVerifiedByOtp(purpose: purpose);
        await unitOfWork.CommitAsync(cancellationToken: cancellationToken);

        return new PublicVerifyOtpResult(IsSuccess: true);
    }
}
