using FluentValidation;
using _116.User.Application.Shared.Validators;

namespace _116.User.Application.Public.UseCases.Commands.VerifyOtp;

/// <summary>
/// Validator for the <see cref="PublicVerifyOtpCommand"/> ensuring proper OTP verification data format.
/// </summary>
/// <remarks>
/// Validates email and OTP code according to format requirements:
/// - Email: Valid email format
/// - Code: 6-digit numeric code
/// </remarks>
public class PublicVerifyOtpValidator : AbstractValidator<PublicVerifyOtpCommand>
{
    /// <summary>
    /// Configure validation rules for OTP verification.
    /// </summary>
    public PublicVerifyOtpValidator()
    {
        // Email validation
        RuleFor(x => x.Email).EmailValidation();

        // OTP code validation
        RuleFor(x => x.Code).OtpCodeValidation();
    }
}
