using _116.Identity.Application.Auth.Validators;
using FluentValidation;

namespace _116.Identity.Application.Auth.UseCases.Admin.Commands.ResendOtp;

/// <summary>
/// Validator for the <see cref="AdminResendOtpCommand" /> ensuring proper resend OTP data format.
/// </summary>
/// <remarks>
/// Validates email format and OTP purpose according to requirements:
/// - Email: Valid email format and required
/// - Purpose: Must be a valid OTP purpose enum value
/// </remarks>
public class AdminResendOtpValidator : AbstractValidator<AdminResendOtpCommand>
{
    /// <summary>
    /// Configure validation rules for admin OTP resend.
    /// </summary>
    public AdminResendOtpValidator()
    {
        RuleFor(x => x.Email).ValidEmail();
        RuleFor(x => x.Purpose).ValidOtpPurpose();
    }
}
