using _116.BuildingBlocks.Constants;
using _116.Identity.Application.Auth.UseCases.Public.Commands.SignUp;
using _116.Unit.Tests.Common.Constants;
using AwesomeAssertions;
using FluentValidation.TestHelper;
using Xunit;

namespace _116.Unit.Tests.Modules.Identity.Application.Auth.UseCases.Public.Commands.SignUp;

/// <summary>
/// Unit tests for <see cref="PublicSignUpValidator"/>.
/// </summary>
public class PublicSignUpValidatorTests
{
    private readonly PublicSignUpValidator _validator = new();

    #region Valid Command Tests

    [Fact]
    public async Task Validate_WithValidCommand_ShouldNotHaveErrors()
    {
        // Arrange
        PublicSignUpCommand command = new(
            Email: TestConstants.User.ValidEmail,
            UserName: TestConstants.User.ValidUserName,
            Password: TestConstants.User.ValidPassword
        );

        // Act
        var result = await _validator.TestValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public async Task Validate_WithMinLengthValues_ShouldNotHaveErrors()
    {
        // Arrange
        PublicSignUpCommand command = new(
            Email: "a@b.c",
            UserName: new string('a', UserConstants.MinUserNameLength),
            Password: "Pass1234" // Min 8 chars with required patterns
        );

        // Act
        var result = await _validator.TestValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    #endregion

    #region Email Validation Tests

    [Fact]
    public async Task Validate_WithNullEmail_ShouldHaveError()
    {
        // Arrange
        PublicSignUpCommand command = new(
            Email: null!,
            UserName: TestConstants.User.ValidUserName,
            Password: TestConstants.User.ValidPassword
        );

        // Act
        var result = await _validator.TestValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public async Task Validate_WithInvalidEmailFormat_ShouldHaveError()
    {
        // Arrange
        PublicSignUpCommand command = new(
            Email: "notanemail",
            UserName: TestConstants.User.ValidUserName,
            Password: TestConstants.User.ValidPassword
        );

        // Act
        var result = await _validator.TestValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    #endregion

    #region UserName Validation Tests

    [Fact]
    public async Task Validate_WithNullUserName_ShouldHaveError()
    {
        // Arrange
        PublicSignUpCommand command = new(
            Email: TestConstants.User.ValidEmail,
            UserName: null!,
            Password: TestConstants.User.ValidPassword
        );

        // Act
        var result = await _validator.TestValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor(x => x.UserName);
    }

    [Fact]
    public async Task Validate_WithTooShortUserName_ShouldHaveError()
    {
        // Arrange
        PublicSignUpCommand command = new(
            Email: TestConstants.User.ValidEmail,
            UserName: "ab", // Less than minimum
            Password: TestConstants.User.ValidPassword
        );

        // Act
        var result = await _validator.TestValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor(x => x.UserName);
    }

    [Fact]
    public async Task Validate_WithTooLongUserName_ShouldHaveError()
    {
        // Arrange
        PublicSignUpCommand command = new(
            Email: TestConstants.User.ValidEmail,
            UserName: new string('a', UserConstants.MaxUserNameLength + 1),
            Password: TestConstants.User.ValidPassword
        );

        // Act
        var result = await _validator.TestValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor(x => x.UserName);
    }

    [Fact]
    public async Task Validate_WithInvalidUserNameCharacters_ShouldHaveError()
    {
        // Arrange
        PublicSignUpCommand command = new(
            Email: TestConstants.User.ValidEmail,
            UserName: "user@name!", // Invalid characters
            Password: TestConstants.User.ValidPassword
        );

        // Act
        var result = await _validator.TestValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor(x => x.UserName);
    }

    #endregion

    #region Password Validation Tests

    [Fact]
    public async Task Validate_WithNullPassword_ShouldHaveError()
    {
        // Arrange
        PublicSignUpCommand command = new(
            Email: TestConstants.User.ValidEmail,
            UserName: TestConstants.User.ValidUserName,
            Password: null!
        );

        // Act
        var result = await _validator.TestValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor(x => x.Password);
    }

    [Fact]
    public async Task Validate_WithTooShortPassword_ShouldHaveError()
    {
        // Arrange
        PublicSignUpCommand command = new(
            Email: TestConstants.User.ValidEmail,
            UserName: TestConstants.User.ValidUserName,
            Password: "Pass1" // Too short
        );

        // Act
        var result = await _validator.TestValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor(x => x.Password);
    }

    [Fact]
    public async Task Validate_WithPasswordMissingLowercase_ShouldHaveError()
    {
        // Arrange
        PublicSignUpCommand command = new(
            Email: TestConstants.User.ValidEmail,
            UserName: TestConstants.User.ValidUserName,
            Password: "PASSWORD123" // No lowercase
        );

        // Act
        var result = await _validator.TestValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor(x => x.Password);
    }

    [Fact]
    public async Task Validate_WithPasswordMissingUppercase_ShouldHaveError()
    {
        // Arrange
        PublicSignUpCommand command = new(
            Email: TestConstants.User.ValidEmail,
            UserName: TestConstants.User.ValidUserName,
            Password: "password123" // No uppercase
        );

        // Act
        var result = await _validator.TestValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor(x => x.Password);
    }

    [Fact]
    public async Task Validate_WithPasswordMissingNumber_ShouldHaveError()
    {
        // Arrange
        PublicSignUpCommand command = new(
            Email: TestConstants.User.ValidEmail,
            UserName: TestConstants.User.ValidUserName,
            Password: "PasswordOnly" // No number
        );

        // Act
        var result = await _validator.TestValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor(x => x.Password);
    }

    #endregion

    #region Multiple Validation Errors Tests

    [Fact]
    public async Task Validate_WithAllInvalidValues_ShouldHaveMultipleErrors()
    {
        // Arrange
        PublicSignUpCommand command = new(Email: "invalid", UserName: "a", Password: "weak");

        // Act
        var result = await _validator.TestValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Count.Should().BeGreaterThanOrEqualTo(3);
        result.ShouldHaveValidationErrorFor(x => x.Email);
        result.ShouldHaveValidationErrorFor(x => x.UserName);
        result.ShouldHaveValidationErrorFor(x => x.Password);
    }

    #endregion
}
