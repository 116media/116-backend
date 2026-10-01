using _116.BuildingBlocks.Application.CQRS;
using _116.Identity.Application.Auth.Services;
using _116.Identity.Application.Shared.Persistence;
using _116.Identity.Application.Shared.Repositories;
using _116.Identity.Domain.Entities;
using _116.Identity.Domain.ValueObjects;

namespace _116.Identity.Application.Auth.UseCases.Admin.Commands.VerifyOtp;

/// <summary>
/// Handles the <see cref="AdminVerifyOtpCommand" /> to verify OTP codes for admin account verification.
/// </summary>
/// <param name="authRepository">Repository for user data access operations.</param>
/// <param name="otpVerificationService">Service consuming the outstanding OTP.</param>
/// <param name="unitOfWork">Unit of Work for managing database transactions.</param>
public class AdminVerifyOtpHandler(
    IAuthRepository authRepository,
    IOtpVerificationService otpVerificationService,
    IIdentityUnitOfWork unitOfWork
) : ICommandHandler<AdminVerifyOtpCommand, AdminVerifyOtpResult>
{
    /// <inheritdoc />
    public async Task<AdminVerifyOtpResult> Handle(AdminVerifyOtpCommand command, CancellationToken cancellationToken)
    {
        var email = new Email(value: command.Email);
        var purpose = new OtpPurpose(value: command.Purpose);
        UserEntity? user = await authRepository.GetUserWithRolesByEmailOrThrow(
            email: email,
            cancellationToken: cancellationToken
        );

        authRepository.IsUserAdmin(user!);
        authRepository.IsUserAccountActive(user!);

        await otpVerificationService.ConsumeOtpAsync(
            userId: user!.Id,
            purpose: purpose,
            code: command.Code,
            cancellationToken: cancellationToken
        );

        user.MarkVerifiedByOtp(purpose: purpose);
        await unitOfWork.CommitAsync(cancellationToken: cancellationToken);
        return new AdminVerifyOtpResult(IsSuccess: true);
    }
}
