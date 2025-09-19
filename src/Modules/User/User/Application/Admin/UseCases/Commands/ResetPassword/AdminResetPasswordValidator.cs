using FluentValidation;
using System.Text.RegularExpressions;
using _116.BuildingBlocks.Constants;

namespace _116.User.Application.Admin.UseCases.Commands.ResetPassword;

/// <summary>
/// Validator for the <see cref="AdminResetPasswordCommand"/> ensuring proper password reset data format.
/// </summary>
/// <remarks>
/// Validates email, OTP code, and new password according to security requirements:
/// - Email: Valid email format and required
/// - Code: 6-digit numeric OTP code
/// - NewPassword: Strong password with mixed cases, numbers, minimum length
/// </remarks>
public partial class AdminResetPasswordValidator : AbstractValidator<AdminResetPasswordCommand>
{
    /// <summary>
    /// Configure validation rules for admin password reset.
    /// </summary>
    public AdminResetPasswordValidator()
    {
        // Email validation
        RuleFor(x => x.Email)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Invalid email format");

        // OTP code validation
        RuleFor(x => x.Code)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("Verification code is required")
            .Length(UserConstants.OtpCodeLength).WithMessage($"Verification code must be exactly {UserConstants.OtpCodeLength} characters long")
            .Matches(OtpCodeRegex())
            .WithMessage("Verification code must contain only numbers");

        // New password validation - strong password requirements
        RuleFor(x => x.NewPassword)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("New password is required")
            .MinimumLength(UserConstants.MinPasswordLength)
            .WithMessage($"Password must be at least {UserConstants.MinPasswordLength} characters long")
            .Matches(PasswordRegex())
            .WithMessage("Password must contain at least one lowercase letter, one uppercase letter, and one number");
    }

    /// <summary>
    /// Generated regex for OTP code validation - 6-digit numeric only.
    /// Uses compile-time generation for better performance, AOT compatibility, and reduced startup time.
    /// </summary>
    [GeneratedRegex(@"^\d{6}$")]
    private static partial Regex OtpCodeRegex();

    /// <summary>
    /// Generated regex for password validation - at least one lowercase, one uppercase, and one number.
    /// Uses compile-time generation for better performance, AOT compatibility, and reduced startup time.
    /// </summary>
    [GeneratedRegex("^(?=.*[a-z])(?=.*[A-Z])(?=.*[0-9])")]
    private static partial Regex PasswordRegex();
}
