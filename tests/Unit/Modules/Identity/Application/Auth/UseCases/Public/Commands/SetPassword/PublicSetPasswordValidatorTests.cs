using _116.BuildingBlocks.Constants;
using _116.Identity.Application.Auth.UseCases.Public.Commands.SetPassword;
using _116.Tests.Fixtures.Constants;
using AwesomeAssertions;
using FluentValidation.TestHelper;
using Xunit;

namespace _116.Unit.Tests.Modules.Identity.Application.Auth.UseCases.Public.Commands.SetPassword;

/// <summary>
/// Unit tests for <see cref="PublicSetPasswordValidator"/>.
/// </summary>
public class PublicSetPasswordValidatorTests
{
    private readonly PublicSetPasswordValidator _validator = new();

    #region Valid Command Tests

    [Fact]
    public async Task Validate_WithValidCommand_ShouldNotHaveErrors()
    {
        // Arrange
        PublicSetPasswordCommand command = new(UserId: Guid.NewGuid(), Password: TestConstants.User.ValidPassword);

        // Act
        TestValidationResult<PublicSetPasswordCommand>? result = await _validator.TestValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    #endregion

    #region Password Validation Tests

    [Fact]
    public async Task Validate_WithNullPassword_ShouldHaveError()
    {
        // Arrange
        PublicSetPasswordCommand command = new(UserId: Guid.NewGuid(), Password: null!);

        // Act
        TestValidationResult<PublicSetPasswordCommand>? result = await _validator.TestValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor(x => x.Password).WithErrorMessage("Password is required");
    }

    [Fact]
    public async Task Validate_WithEmptyPassword_ShouldHaveError()
    {
        // Arrange
        PublicSetPasswordCommand command = new(UserId: Guid.NewGuid(), Password: string.Empty);

        // Act
        TestValidationResult<PublicSetPasswordCommand>? result = await _validator.TestValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor(x => x.Password);
    }

    [Fact]
    public async Task Validate_WithTooShortPassword_ShouldHaveError()
    {
        // Arrange
        PublicSetPasswordCommand command = new(UserId: Guid.NewGuid(), Password: "Pass1");

        // Act
        TestValidationResult<PublicSetPasswordCommand>? result = await _validator.TestValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .ShouldHaveValidationErrorFor(x => x.Password)
            .WithErrorMessage($"Password must be at least {UserConstants.MinPasswordLength} characters long");
    }

    [Fact]
    public async Task Validate_WithPasswordMissingLowercase_ShouldHaveError()
    {
        // Arrange
        PublicSetPasswordCommand command = new(UserId: Guid.NewGuid(), Password: "PASSWORD123");

        // Act
        TestValidationResult<PublicSetPasswordCommand>? result = await _validator.TestValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor(x => x.Password);
    }

    [Fact]
    public async Task Validate_WithPasswordMissingUppercase_ShouldHaveError()
    {
        // Arrange
        PublicSetPasswordCommand command = new(UserId: Guid.NewGuid(), Password: "password123");

        // Act
        TestValidationResult<PublicSetPasswordCommand>? result = await _validator.TestValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor(x => x.Password);
    }

    [Fact]
    public async Task Validate_WithPasswordMissingNumber_ShouldHaveError()
    {
        // Arrange
        PublicSetPasswordCommand command = new(UserId: Guid.NewGuid(), Password: "PasswordOnly");

        // Act
        TestValidationResult<PublicSetPasswordCommand>? result = await _validator.TestValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor(x => x.Password);
    }

    #endregion
}
