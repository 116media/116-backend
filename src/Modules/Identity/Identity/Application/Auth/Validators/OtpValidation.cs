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
    /// <returns>The configured rule builder.</returns>
    public static IRuleBuilderOptions<T, string> ValidOtpCode<T>(
        this IRuleBuilderInitial<T, string> ruleBuilder
    )
    {
        return ruleBuilder
            .Cascade(cascadeMode: CascadeMode.Stop)
            .NotEmpty().WithMessage("Verification code is required")
            .Length(exactLength: UserConstants.OtpCodeLength)
            .WithMessage($"Verification code must be exactly {UserConstants.OtpCodeLength} characters long")
            .Matches(OtpCodeRegex())
            .WithMessage("Verification code must contain only numbers");
    }

    /// <summary>
    /// Validates OTP purpose enum value.
    /// </summary>
    /// <typeparam name="T">The type being validated.</typeparam>
    /// <param name="ruleBuilder">The rule builder for the OTP purpose property.</param>
    /// <returns>The configured rule builder.</returns>
    public static IRuleBuilderOptions<T, string> ValidOtpPurpose<T>(
        this IRuleBuilderInitial<T, string> ruleBuilder
    )
    {
        return ruleBuilder
            .Cascade(cascadeMode: CascadeMode.Stop)
            .NotEmpty().WithMessage("OTP purpose is required.")
            .Must(purpose => purpose != null && Enum.IsDefined(typeof(EnumOtpPurpose), value: purpose))
            .WithMessage("Invalid OTP purpose specified.");
    }

    /// <summary>
    /// Generated regex for OTP code validation - 6-digit numeric only.
    /// Uses compile-time generation for better performance, AOT compatibility, and reduced startup time.
    /// </summary>
    [GeneratedRegex(@"^\d{6}$")]
    private static partial Regex OtpCodeRegex();
}
