using _116.BuildingBlocks.Constants;
using _116.Identity.Application.Auth.Validators;
using _116.Identity.Application.Shared.Errors.Messages;
using FluentValidation;

namespace _116.Identity.Application.Auth.UseCases.Admin.Commands.VerifyOtp;

/// <summary>
/// Validator for the <see cref="AdminVerifyOtpCommand" /> ensuring proper OTP verification data format.
/// </summary>
/// <remarks>
/// Validates email, OTP code, and purpose according to format requirements:
/// - Email: Valid email format
/// - Code: 6-digit numeric code
/// - Purpose: Must be EmailVerification or AccountRecovery
/// </remarks>
public class AdminVerifyOtpValidator : AbstractValidator<AdminVerifyOtpCommand>
{
    /// <summary>
    /// Initializes a new instance of <see cref="AdminVerifyOtpValidator" /> with validation rules.
    /// </summary>
    /// <param name="msg">
    /// Validation error messages for rule configuration.
    /// </param>
    public AdminVerifyOtpValidator(ValidationErrorMessage msg)
    {
        RuleFor(x => x.Email)
            .ValidEmail(
                emailRequired: msg.EmailRequired(),
                emailTooLong: msg.EmailTooLong(UserConstants.MaxEmailLength),
                invalidEmailFormat: msg.InvalidEmailFormatMsg()
            );
        RuleFor(x => x.Code)
            .ValidOtpCode(
                otpCodeRequired: msg.OtpCodeRequired(),
                otpCodeWrongLength: msg.OtpCodeWrongLength(UserConstants.OtpCodeLength),
                otpCodeNotNumeric: msg.OtpCodeNotNumeric()
            );
        RuleFor(x => x.Purpose)
            .ValidOtpPurpose(otpPurposeRequired: msg.OtpPurposeRequired(), otpPurposeInvalid: msg.OtpPurposeInvalid());
    }
}
