using _116.Identity.Application.Shared.Persistence;
using _116.Identity.Application.Shared.Repositories;
using _116.Identity.Application.Shared.Services;
using _116.Identity.Domain.Entities;
using _116.Identity.Domain.ValueObjects;
using _116.Shared.Application.Exceptions;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Identity.Application.Auth.Public.UseCases.Commands.ResendOtp;

/// <summary>
/// Handles the <see cref="PublicResendOtpCommand"/> to resend OTP codes for public users.
/// </summary>
public class PublicResendOtpHandler(
    IAuthRepository authRepository,
    IOtpRepository otpRepository,
    IOtpService otpService,
    IIdentityUnitOfWork unitOfWork
) : ICommandHandler<PublicResendOtpCommand, PublicResendOtpResult>
{
    /// <summary>
    /// Handles the resend OTP command by invalidating existing OTPs and generating a new one.
    /// </summary>
    /// <param name="command">The resend OTP command containing email and purpose.</param>
    /// <param name="cancellationToken">Token to observe for cancellation requests.</param>
    /// <returns>The result indicating success or failure of the OTP resend operation.</returns>
    /// <exception cref="NotFoundException">Thrown when the user is not found.</exception>
    /// <exception cref="BadRequestException">Thrown when the user account is inactive or not verified.</exception>
    public async Task<PublicResendOtpResult> Handle(
        PublicResendOtpCommand command,
        CancellationToken cancellationToken
    )
    {
        var email = new Email(command.Email);
        var purpose = new OtpPurpose(command.Purpose);
        if (!await authRepository.ExistsByEmailAsync(email, cancellationToken))
        {
            return new PublicResendOtpResult(IsSuccess: true);
        }
        UserEntity? user = await authRepository.GetUserWithRolesByEmailOrThrow(email, cancellationToken);
        authRepository.IsUserAccountActive(user!);
        // Invalidate existing OTPs for this purpose
        await otpRepository.InvalidateExistingOtpsAsync(user!.Id, purpose, cancellationToken);
        // Create new OTP
        OtpEntity newOtp = otpService.CreateOtp(user.Id, purpose);
        // Save the new OTP
        await otpRepository.AddAsync(newOtp, cancellationToken);
        await unitOfWork.CommitAsync(cancellationToken);
        return new PublicResendOtpResult(IsSuccess: true);
    }
}
