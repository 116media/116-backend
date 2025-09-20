using FluentValidation;
using _116.User.Application.Shared.Validators;
using _116.User.Domain.Enums;

namespace _116.User.Application.Admin.UseCases.Commands.ResendOtp;

/// <summary>
/// Validator for the <see cref="AdminResendOtpCommand"/> ensuring proper resend OTP data format.
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
        // Email validation
        RuleFor(x => x.Email).EmailValidation();

        // Purpose validation - must be a valid enum value
        RuleFor(x => x.Purpose).OtpPurposeValidation();
    }
}
