using System.Text.RegularExpressions;
using _116.BuildingBlocks.Constants;
using _116.Identity.Application.Shared.Errors.Messages;
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
    /// <param name="i18n">Validation error messages for rule configuration.</param>
    /// <returns>The configured rule builder.</returns>
    public static IRuleBuilderOptions<T, string> ValidOtpCode<T>(
        this IRuleBuilderInitial<T, string> ruleBuilder,
        ValidationErrorMessage i18n
    )
    {
        return ruleBuilder
            .Cascade(cascadeMode: CascadeMode.Stop)
            .NotEmpty()
            .WithMessage(i18n.OtpCodeRequired())
            .Length(exactLength: UserConstants.OtpCodeLength)
            .WithMessage(i18n.OtpCodeWrongLength(UserConstants.OtpCodeLength))
            .Matches(OtpCodeRegex())
            .WithMessage(i18n.OtpCodeNotNumeric());
    }

    /// <summary>
    /// Validates OTP purpose enum value.
    /// </summary>
    /// <typeparam name="T">The type being validated.</typeparam>
    /// <param name="ruleBuilder">The rule builder for the OTP purpose property.</param>
    /// <param name="i18n">Validation error messages for rule configuration.</param>
    /// <returns>The configured rule builder.</returns>
    public static IRuleBuilderOptions<T, string> ValidOtpPurpose<T>(
        this IRuleBuilderInitial<T, string> ruleBuilder,
        ValidationErrorMessage i18n
    )
    {
        return ruleBuilder
            .Cascade(cascadeMode: CascadeMode.Stop)
            .NotEmpty()
            .WithMessage(i18n.OtpPurposeRequired())
            .Must(purpose => purpose != null && Enum.IsDefined(typeof(EnumOtpPurpose), value: purpose))
            .WithMessage(i18n.OtpPurposeInvalid());
    }

    /// <summary>
    /// Generated regex for OTP code validation - 6-digit numeric only.
    /// Uses compile-time generation for better performance, AOT compatibility, and reduced startup time.
    /// </summary>
    [GeneratedRegex(@"^\d{6}$")]
    private static partial Regex OtpCodeRegex();
}
