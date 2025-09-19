using _116.Shared.Application.Exceptions;
using _116.Shared.Contracts.Application.CQRS;
using _116.User.Application.Shared.Repositories;
using _116.User.Application.Shared.Services;
using _116.User.Domain.Entities;
using _116.User.Domain.ValueObjects;

namespace _116.User.Application.Admin.UseCases.Commands.ResendOtp;

/// <summary>
/// Handles the <see cref="AdminResendOtpCommand"/> to resend OTP codes for admin users.
/// </summary>
public class AdminResendOtpHandler(
    IUserRepository userRepository,
    IOtpRepository otpRepository,
    IOtpService otpService
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

        // Verify admin user exists
        if (!await userRepository.ExistsByEmailAsync(email, cancellationToken))
        {
            return new AdminResendOtpResult(IsSuccess: true);
        }

        // Get the admin user with roles
        UserEntity user = await userRepository.GetUserWithRolesOrThrowAsync(email, cancellationToken);

        userRepository.IsUserAccountActive(user);

        // Invalidate existing OTPs for this purpose
        await otpRepository.InvalidateExistingOtpsAsync(user.Id, command.Purpose, cancellationToken);

        // Create new OTP
        OtpEntity newOtp = otpService.CreateOtp(user.Id, command.Purpose);

        // Save the new OTP
        await otpRepository.AddAsync(newOtp, cancellationToken);
        await otpRepository.SaveChangesAsync(cancellationToken);

        return new AdminResendOtpResult(IsSuccess: true);
    }
}
