using _116.BuildingBlocks.Constants;
using _116.Identity.Application.Auth.UseCases.Admin.Commands.Login;
using _116.Tests.Fixtures.Constants;
using AwesomeAssertions;
using FluentValidation.TestHelper;
using Xunit;

namespace _116.Unit.Tests.Modules.Identity.Application.Auth.UseCases.Admin.Commands.Login;

/// <summary>
/// Unit tests for <see cref="AdminLoginValidator"/>.
/// </summary>
public class AdminLoginValidatorTests
{
    private readonly AdminLoginValidator _validator = new();

    #region Valid Command Tests

    [Fact]
    public async Task Validate_WithValidCommand_ShouldNotHaveErrors()
    {
        // Arrange
        AdminLoginCommand command = new(
            Email: TestConstants.User.ValidEmail,
            Password: TestConstants.User.ValidPassword
        );

        // Act
        TestValidationResult<AdminLoginCommand>? result = await _validator.TestValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public async Task Validate_WithMaxLengthEmail_ShouldNotHaveErrors()
    {
        // Arrange
        string maxEmail = new string('a', UserConstants.MaxEmailLength - "@test.com".Length) + "@test.com";
        AdminLoginCommand command = new(Email: maxEmail, Password: TestConstants.User.ValidPassword);

        // Act
        TestValidationResult<AdminLoginCommand>? result = await _validator.TestValidateAsync(command);

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
        AdminLoginCommand command = new(Email: null!, Password: TestConstants.User.ValidPassword);

        // Act
        TestValidationResult<AdminLoginCommand>? result = await _validator.TestValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor(x => x.Email).WithErrorMessage("Email is required");
    }

    [Fact]
    public async Task Validate_WithEmptyEmail_ShouldHaveError()
    {
        // Arrange
        AdminLoginCommand command = new(Email: string.Empty, Password: TestConstants.User.ValidPassword);

        // Act
        TestValidationResult<AdminLoginCommand>? result = await _validator.TestValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor(x => x.Email).WithErrorMessage("Email is required");
    }

    [Fact]
    public async Task Validate_WithWhitespaceEmail_ShouldHaveError()
    {
        // Arrange
        AdminLoginCommand command = new(Email: "   ", Password: TestConstants.User.ValidPassword);

        // Act
        TestValidationResult<AdminLoginCommand>? result = await _validator.TestValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor(x => x.Email).WithErrorMessage("Email is required");
    }

    [Fact]
    public async Task Validate_WithInvalidEmailFormat_ShouldHaveError()
    {
        // Arrange
        AdminLoginCommand command = new(Email: "notanemail", Password: TestConstants.User.ValidPassword);

        // Act
        TestValidationResult<AdminLoginCommand>? result = await _validator.TestValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor(x => x.Email).WithErrorMessage("Invalid email format");
    }

    [Fact]
    public async Task Validate_WithEmailExceedingMaxLength_ShouldHaveError()
    {
        // Arrange
        string longEmail = new string('a', UserConstants.MaxEmailLength + 1) + "@test.com";
        AdminLoginCommand command = new(Email: longEmail, Password: TestConstants.User.ValidPassword);

        // Act
        TestValidationResult<AdminLoginCommand>? result = await _validator.TestValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .ShouldHaveValidationErrorFor(x => x.Email)
            .WithErrorMessage($"Email cannot exceed {UserConstants.MaxEmailLength} characters");
    }

    #endregion

    #region Password Validation Tests

    [Fact]
    public async Task Validate_WithNullPassword_ShouldHaveError()
    {
        // Arrange
        AdminLoginCommand command = new(Email: TestConstants.User.ValidEmail, Password: null!);

        // Act
        TestValidationResult<AdminLoginCommand>? result = await _validator.TestValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor(x => x.Password).WithErrorMessage("Password cannot be empty.");
    }

    [Fact]
    public async Task Validate_WithEmptyPassword_ShouldHaveError()
    {
        // Arrange
        AdminLoginCommand command = new(Email: TestConstants.User.ValidEmail, Password: string.Empty);

        // Act
        TestValidationResult<AdminLoginCommand>? result = await _validator.TestValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor(x => x.Password).WithErrorMessage("Password cannot be empty.");
    }

    [Fact]
    public async Task Validate_WithWhitespacePassword_ShouldHaveError()
    {
        // Arrange
        AdminLoginCommand command = new(Email: TestConstants.User.ValidEmail, Password: "   ");

        // Act
        TestValidationResult<AdminLoginCommand>? result = await _validator.TestValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor(x => x.Password).WithErrorMessage("Password cannot be empty.");
    }

    #endregion

    #region Multiple Validation Errors Tests

    [Fact]
    public async Task Validate_WithAllInvalidValues_ShouldHaveMultipleErrors()
    {
        // Arrange
        AdminLoginCommand command = new(Email: string.Empty, Password: string.Empty);

        // Act
        TestValidationResult<AdminLoginCommand>? result = await _validator.TestValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCountGreaterThanOrEqualTo(2);
        result.ShouldHaveValidationErrorFor(x => x.Email);
        result.ShouldHaveValidationErrorFor(x => x.Password);
    }

    #endregion
}
