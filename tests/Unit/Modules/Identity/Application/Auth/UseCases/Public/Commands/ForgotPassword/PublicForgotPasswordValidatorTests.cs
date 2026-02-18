using _116.BuildingBlocks.Constants;
using _116.Identity.Application.Auth.UseCases.Public.Commands.ForgotPassword;
using _116.Tests.Fixtures.Constants;
using AwesomeAssertions;
using FluentValidation.TestHelper;
using Xunit;

namespace _116.Unit.Tests.Modules.Identity.Application.Auth.UseCases.Public.Commands.ForgotPassword;

/// <summary>
/// Unit tests for <see cref="PublicForgotPasswordValidator"/>.
/// </summary>
public class PublicForgotPasswordValidatorTests
{
    private readonly PublicForgotPasswordValidator _validator = new();

    #region Valid Command Tests

    [Fact]
    public async Task Validate_WithValidCommand_ShouldNotHaveErrors()
    {
        // Arrange
        PublicForgotPasswordCommand command = new(Email: TestConstants.User.ValidEmail);

        // Act
        TestValidationResult<PublicForgotPasswordCommand>? result = await _validator.TestValidateAsync(command);

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
        PublicForgotPasswordCommand command = new(Email: null!);

        // Act
        TestValidationResult<PublicForgotPasswordCommand>? result = await _validator.TestValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public async Task Validate_WithEmptyEmail_ShouldHaveError()
    {
        // Arrange
        PublicForgotPasswordCommand command = new(Email: string.Empty);

        // Act
        TestValidationResult<PublicForgotPasswordCommand>? result = await _validator.TestValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public async Task Validate_WithInvalidEmailFormat_ShouldHaveError()
    {
        // Arrange
        PublicForgotPasswordCommand command = new(Email: "notanemail");

        // Act
        TestValidationResult<PublicForgotPasswordCommand>? result = await _validator.TestValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public async Task Validate_WithEmailExceedingMaxLength_ShouldHaveError()
    {
        // Arrange
        string longEmail = new string('a', UserConstants.MaxEmailLength + 1) + "@test.com";
        PublicForgotPasswordCommand command = new(Email: longEmail);

        // Act
        TestValidationResult<PublicForgotPasswordCommand>? result = await _validator.TestValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    #endregion
}
