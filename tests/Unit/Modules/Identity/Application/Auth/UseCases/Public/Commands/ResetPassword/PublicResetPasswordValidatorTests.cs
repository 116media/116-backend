using _116.BuildingBlocks.Constants;
using _116.Identity.Application.Auth.UseCases.Public.Commands.ResetPassword;
using _116.Unit.Tests.Common.Constants;
using AwesomeAssertions;
using FluentValidation.TestHelper;
using Xunit;

namespace _116.Unit.Tests.Modules.Identity.Application.Auth.UseCases.Public.Commands.ResetPassword;

/// <summary>
/// Unit tests for <see cref="PublicResetPasswordValidator"/>.
/// </summary>
public class PublicResetPasswordValidatorTests
{
    private readonly PublicResetPasswordValidator _validator = new();

    #region Valid Command Tests

    [Fact]
    public async Task Validate_WithValidCommand_ShouldNotHaveErrors()
    {
        // Arrange
        PublicResetPasswordCommand command = new(
            Email: TestConstants.User.ValidEmail,
            Code: TestConstants.Otp.ValidCode,
            NewPassword: TestConstants.User.ValidPassword
        );

        // Act
        var result = await _validator.TestValidateAsync(command);

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
        PublicResetPasswordCommand command = new(
            Email: null!,
            Code: TestConstants.Otp.ValidCode,
            NewPassword: TestConstants.User.ValidPassword
        );

        // Act
        var result = await _validator.TestValidateAsync(command);

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
        PublicResetPasswordCommand command = new(
            Email: TestConstants.User.ValidEmail,
            Code: null!,
            NewPassword: TestConstants.User.ValidPassword
        );

        // Act
        var result = await _validator.TestValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor(x => x.Code);
    }

    [Fact]
    public async Task Validate_WithInvalidCodeLength_ShouldHaveError()
    {
        // Arrange
        PublicResetPasswordCommand command = new(
            Email: TestConstants.User.ValidEmail,
            Code: "12345",
            NewPassword: TestConstants.User.ValidPassword
        );

        // Act
        var result = await _validator.TestValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor(x => x.Code);
    }

    [Fact]
    public async Task Validate_WithNonNumericCode_ShouldHaveError()
    {
        // Arrange
        PublicResetPasswordCommand command = new(
            Email: TestConstants.User.ValidEmail,
            Code: "ABC123",
            NewPassword: TestConstants.User.ValidPassword
        );

        // Act
        var result = await _validator.TestValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor(x => x.Code);
    }

    #endregion

    #region NewPassword Validation Tests

    [Fact]
    public async Task Validate_WithNullNewPassword_ShouldHaveError()
    {
        // Arrange
        PublicResetPasswordCommand command = new(
            Email: TestConstants.User.ValidEmail,
            Code: TestConstants.Otp.ValidCode,
            NewPassword: null!
        );

        // Act
        var result = await _validator.TestValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor(x => x.NewPassword);
    }

    [Fact]
    public async Task Validate_WithWeakNewPassword_ShouldHaveError()
    {
        // Arrange
        PublicResetPasswordCommand command = new(
            Email: TestConstants.User.ValidEmail,
            Code: TestConstants.Otp.ValidCode,
            NewPassword: "weakpassword"
        );

        // Act
        var result = await _validator.TestValidateAsync(command);

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
        PublicResetPasswordCommand command = new(Email: "invalid", Code: "abc", NewPassword: "weak");

        // Act
        var result = await _validator.TestValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Count.Should().BeGreaterThanOrEqualTo(3);
    }

    #endregion
}
