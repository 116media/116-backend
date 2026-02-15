using _116.BuildingBlocks.Constants;
using _116.Identity.Application.Auth.UseCases.Admin.Commands.ResetPassword;
using _116.Unit.Tests.Common.Constants;
using AwesomeAssertions;
using FluentValidation.TestHelper;
using Xunit;

namespace _116.Unit.Tests.Modules.Identity.Application.Auth.UseCases.Admin.Commands.ResetPassword;

/// <summary>
/// Unit tests for <see cref="AdminResetPasswordValidator"/>.
/// </summary>
public class AdminResetPasswordValidatorTests
{
    private readonly AdminResetPasswordValidator _validator = new();

    #region Valid Command Tests

    [Fact]
    public async Task Validate_WithValidCommand_ShouldNotHaveErrors()
    {
        // Arrange
        AdminResetPasswordCommand command = new(
            Email: TestConstants.User.ValidEmail,
            Code: TestConstants.Otp.ValidCode,
            NewPassword: TestConstants.User.ValidPassword
        );

        // Act
        TestValidationResult<AdminResetPasswordCommand>? result = await _validator.TestValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    #endregion

    #region Email Validation Tests

    [Fact]
    public async Task Validate_WithNullEmail_ShouldHaveError()
    {
        // Arrange
        AdminResetPasswordCommand command = new(
            Email: null!,
            Code: TestConstants.Otp.ValidCode,
            NewPassword: TestConstants.User.ValidPassword
        );

        // Act
        TestValidationResult<AdminResetPasswordCommand>? result = await _validator.TestValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public async Task Validate_WithInvalidEmailFormat_ShouldHaveError()
    {
        // Arrange
        AdminResetPasswordCommand command = new(
            Email: "notanemail",
            Code: TestConstants.Otp.ValidCode,
            NewPassword: TestConstants.User.ValidPassword
        );

        // Act
        TestValidationResult<AdminResetPasswordCommand>? result = await _validator.TestValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    #endregion

    #region Code Validation Tests

    [Fact]
    public async Task Validate_WithNullCode_ShouldHaveError()
    {
        // Arrange
        AdminResetPasswordCommand command = new(
            Email: TestConstants.User.ValidEmail,
            Code: null!,
            NewPassword: TestConstants.User.ValidPassword
        );

        // Act
        TestValidationResult<AdminResetPasswordCommand>? result = await _validator.TestValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor(x => x.Code);
    }

    [Fact]
    public async Task Validate_WithTooShortCode_ShouldHaveError()
    {
        // Arrange
        AdminResetPasswordCommand command = new(
            Email: TestConstants.User.ValidEmail,
            Code: "12345", // Too short
            NewPassword: TestConstants.User.ValidPassword
        );

        // Act
        TestValidationResult<AdminResetPasswordCommand>? result = await _validator.TestValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .ShouldHaveValidationErrorFor(x => x.Code)
            .WithErrorMessage($"Verification code must be exactly {UserConstants.OtpCodeLength} characters long");
    }

    [Fact]
    public async Task Validate_WithTooLongCode_ShouldHaveError()
    {
        // Arrange
        AdminResetPasswordCommand command = new(
            Email: TestConstants.User.ValidEmail,
            Code: "1234567", // Too long
            NewPassword: TestConstants.User.ValidPassword
        );

        // Act
        TestValidationResult<AdminResetPasswordCommand>? result = await _validator.TestValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor(x => x.Code);
    }

    [Fact]
    public async Task Validate_WithNonNumericCode_ShouldHaveError()
    {
        // Arrange
        AdminResetPasswordCommand command = new(
            Email: TestConstants.User.ValidEmail,
            Code: "ABC123", // Contains letters
            NewPassword: TestConstants.User.ValidPassword
        );

        // Act
        TestValidationResult<AdminResetPasswordCommand>? result = await _validator.TestValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .ShouldHaveValidationErrorFor(x => x.Code)
            .WithErrorMessage("Verification code must contain only numbers");
    }

    #endregion

    #region NewPassword Validation Tests

    [Fact]
    public async Task Validate_WithNullNewPassword_ShouldHaveError()
    {
        // Arrange
        AdminResetPasswordCommand command = new(
            Email: TestConstants.User.ValidEmail,
            Code: TestConstants.Otp.ValidCode,
            NewPassword: null!
        );

        // Act
        TestValidationResult<AdminResetPasswordCommand>? result = await _validator.TestValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor(x => x.NewPassword);
    }

    [Fact]
    public async Task Validate_WithTooShortNewPassword_ShouldHaveError()
    {
        // Arrange
        AdminResetPasswordCommand command = new(
            Email: TestConstants.User.ValidEmail,
            Code: TestConstants.Otp.ValidCode,
            NewPassword: "Pass1"
        );

        // Act
        TestValidationResult<AdminResetPasswordCommand>? result = await _validator.TestValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor(x => x.NewPassword);
    }

    [Fact]
    public async Task Validate_WithWeakNewPassword_ShouldHaveError()
    {
        // Arrange
        AdminResetPasswordCommand command = new(
            Email: TestConstants.User.ValidEmail,
            Code: TestConstants.Otp.ValidCode,
            NewPassword: "weakpassword"
        );

        // Act
        TestValidationResult<AdminResetPasswordCommand>? result = await _validator.TestValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor(x => x.NewPassword);
    }

    #endregion

    #region Multiple Validation Errors Tests

    [Fact]
    public async Task Validate_WithAllInvalidValues_ShouldHaveMultipleErrors()
    {
        // Arrange
        AdminResetPasswordCommand command = new(Email: "invalid", Code: "abc", NewPassword: "weak");

        // Act
        TestValidationResult<AdminResetPasswordCommand>? result = await _validator.TestValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCountGreaterThanOrEqualTo(3);
    }

    #endregion
}
