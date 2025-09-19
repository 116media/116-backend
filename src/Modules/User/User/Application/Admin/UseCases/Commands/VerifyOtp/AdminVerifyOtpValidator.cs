using FluentValidation;
using _116.User.Application.Shared.Validators;

namespace _116.User.Application.Admin.UseCases.Commands.VerifyOtp;

/// <summary>
/// Validator for the <see cref="AdminVerifyOtpCommand"/> ensuring proper OTP verification data format.
/// </summary>
/// <remarks>
/// Validates email and OTP code according to format requirements:
/// - Email: Valid email format
/// - Code: 6-digit numeric code
/// </remarks>
public class AdminVerifyOtpValidator : AbstractValidator<AdminVerifyOtpCommand>
{
    /// <summary>
    /// Configure validation rules for admin OTP verification.
    /// </summary>
    public AdminVerifyOtpValidator()
    {
        // Email validation
        RuleFor(x => x.Email).EmailValidation();

        // OTP code validation
        RuleFor(x => x.Code).OtpCodeValidation();
    }
}
