using _116.Shared.Application.Exceptions;
using _116.Shared.Application.Persistence;
using _116.Shared.Contracts.Application.CQRS;
using _116.Auth.Application.Shared.Repositories;
using _116.Auth.Application.Shared.Services;
using _116.Auth.Domain.Entities;
using _116.Auth.Domain.ValueObjects;

namespace _116.Auth.Application.Admin.UseCases.Commands.ResendOtp;

/// <summary>
/// Handles the <see cref="AdminResendOtpCommand"/> to resend OTP codes for admin users.
/// </summary>
public class AdminResendOtpHandler(
    IUserRepository userRepository,
    IOtpRepository otpRepository,
    IOtpService otpService,
    IUnitOfWork unitOfWork
) : ICommandHandler<AdminResendOtpCommand, AdminResendOtpResult>
{
    /// <summary>
    /// Handles the resend OTP command by invalidating existing OTPs and generating a new one.
    /// </summary>
    /// <param name="command">The resend OTP command containing email and purpose.</param>
    /// <param name="cancellationToken">Token to observe for cancellation requests.</param>
    /// <returns>The result indicating success or failure of the OTP resend operation.</returns>
    /// <exception cref="NotFoundException">Thrown when the admin user is not found.</exception>
    /// <exception cref="BadRequestException">Thrown when the admin account is inactive or not verified.</exception>
    public async Task<AdminResendOtpResult> Handle(
        AdminResendOtpCommand command,
        CancellationToken cancellationToken
    )
    {
        var email = new Email(command.Email);
        var purpose = new OtpPurpose(command.Purpose);

        if (!await userRepository.ExistsByEmailAsync(email, cancellationToken))
        {
            return new AdminResendOtpResult(IsSuccess: true);
        }

        UserEntity? user = await userRepository.GetUserWithRolesByEmailOrThrow(email, cancellationToken);

        userRepository.IsUserAdmin(user!);
        userRepository.IsUserAccountActive(user!);

        // Invalidate existing OTPs for this purpose
        await otpRepository.InvalidateExistingOtpsAsync(user!.Id, purpose, cancellationToken);

        // Create new OTP
        OtpEntity newOtp = otpService.CreateOtp(user.Id, purpose);

        // Save the new OTP
        await otpRepository.AddAsync(newOtp, cancellationToken);
        await unitOfWork.CommitAsync(cancellationToken);

        return new AdminResendOtpResult(IsSuccess: true);
    }
}
