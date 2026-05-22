using System.Text.RegularExpressions;
using _116.BuildingBlocks.Constants;
using _116.Identity.Domain.Enums;
using FluentValidation;

namespace _116.Identity.Application.Auth.Validators;

/// <summary>
/// FluentValidation extensions for OTP (One-Time Password) validation.
/// </summary>
public static partial class OtpValidation
{
    /// <summary>
    /// Validates OTP code format (6-digit numeric).
    /// </summary>
    /// <typeparam name="T">The type being validated.</typeparam>
    /// <param name="ruleBuilder">The rule builder for the OTP code property.</param>
    /// <param name="otpCodeRequired">Error message when OTP code is missing.</param>
    /// <param name="otpCodeWrongLength">Error message when OTP code has wrong length.</param>
    /// <param name="otpCodeNotNumeric">Error message when OTP code contains non-numeric characters.</param>
    /// <returns>The configured rule builder.</returns>
    public static IRuleBuilderOptions<T, string> ValidOtpCode<T>(
        this IRuleBuilderInitial<T, string> ruleBuilder,
        string otpCodeRequired,
        string otpCodeWrongLength,
        string otpCodeNotNumeric
    )
    {
        return ruleBuilder
            .Cascade(cascadeMode: CascadeMode.Stop)
            .NotEmpty()
            .WithMessage(otpCodeRequired)
            .Length(exactLength: UserConstants.OtpCodeLength)
            .WithMessage(otpCodeWrongLength)
            .Matches(OtpCodeRegex())
            .WithMessage(otpCodeNotNumeric);
    }

    /// <summary>
    /// Validates OTP purpose enum value.
    /// </summary>
    /// <typeparam name="T">The type being validated.</typeparam>
    /// <param name="ruleBuilder">The rule builder for the OTP purpose property.</param>
    /// <param name="otpPurposeRequired">Error message when OTP purpose is missing.</param>
    /// <param name="otpPurposeInvalid">Error message when OTP purpose is not a valid enum value.</param>
    /// <returns>The configured rule builder.</returns>
    public static IRuleBuilderOptions<T, string> ValidOtpPurpose<T>(
        this IRuleBuilderInitial<T, string> ruleBuilder,
        string otpPurposeRequired,
        string otpPurposeInvalid
    )
    {
        return ruleBuilder
            .Cascade(cascadeMode: CascadeMode.Stop)
            .NotEmpty()
            .WithMessage(otpPurposeRequired)
            .Must(purpose => purpose != null && Enum.IsDefined(typeof(EnumOtpPurpose), value: purpose))
            .WithMessage(otpPurposeInvalid);
    }

    /// <summary>
    /// Generated regex for OTP code validation - 6-digit numeric only.
    /// Uses compile-time generation for better performance, AOT compatibility, and reduced startup time.
    /// </summary>
    [GeneratedRegex(@"^\d{6}$")]
    private static partial Regex OtpCodeRegex();
}
