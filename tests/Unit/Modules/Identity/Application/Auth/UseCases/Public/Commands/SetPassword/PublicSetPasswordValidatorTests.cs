using _116.BuildingBlocks.Constants;
using _116.Identity.Application.Auth.UseCases.Public.Commands.SetPassword;
using _116.Identity.Application.Shared.Errors.Facade;
using _116.Tests.Fixtures.Constants;
using _116.Tests.Fixtures.Helpers;
using AwesomeAssertions;
using FluentValidation.TestHelper;
using Xunit;

namespace _116.Unit.Tests.Modules.Identity.Application.Auth.UseCases.Public.Commands.SetPassword;

/// <summary>
/// Unit tests for <see cref="PublicSetPasswordValidator"/>.
/// </summary>
public class PublicSetPasswordValidatorTests
{
    private readonly IdentityI18n _i18n = TestErrorsFactory.CreateIdentityI18n();
    private readonly PublicSetPasswordValidator _validator;

    /// <summary>
    /// Initializes a new instance of <see cref="PublicSetPasswordValidatorTests"/>.
    /// </summary>
    public PublicSetPasswordValidatorTests()
    {
        _validator = new PublicSetPasswordValidator(_i18n);
    }

    #region Valid Command Tests

    [Fact]
    public async Task Validate_WithValidCommand_ShouldNotHaveErrors()
    {
        // Arrange
        PublicSetPasswordCommand command = new(
            UserId: Guid.NewGuid(),
            SessionId: Guid.NewGuid(),
            Password: TestConstants.User.ValidPassword
        );

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
        PublicSetPasswordCommand command = new(UserId: Guid.NewGuid(), SessionId: Guid.NewGuid(), Password: null!);

        // Act
        TestValidationResult<PublicSetPasswordCommand>? result = await _validator.TestValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor(x => x.Password).WithErrorMessage(_i18n.User.Validation.PasswordRequired());
    }

    [Fact]
    public async Task Validate_WithEmptyPassword_ShouldHaveError()
    {
        // Arrange
        PublicSetPasswordCommand command = new(
            UserId: Guid.NewGuid(),
            SessionId: Guid.NewGuid(),
            Password: string.Empty
        );

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
        PublicSetPasswordCommand command = new(UserId: Guid.NewGuid(), SessionId: Guid.NewGuid(), Password: "Pass1");

        // Act
        TestValidationResult<PublicSetPasswordCommand>? result = await _validator.TestValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .ShouldHaveValidationErrorFor(x => x.Password)
            .WithErrorMessage(_i18n.User.Validation.PasswordTooShort("Password", UserConstants.MinPasswordLength));
    }

    [Fact]
    public async Task Validate_WithPasswordMissingLowercase_ShouldHaveError()
    {
        // Arrange
        PublicSetPasswordCommand command = new(
            UserId: Guid.NewGuid(),
            SessionId: Guid.NewGuid(),
            Password: "PASSWORD123"
        );

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
        PublicSetPasswordCommand command = new(
            UserId: Guid.NewGuid(),
            SessionId: Guid.NewGuid(),
            Password: "password123"
        );

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
        PublicSetPasswordCommand command = new(
            UserId: Guid.NewGuid(),
            SessionId: Guid.NewGuid(),
            Password: "PasswordOnly"
        );

        // Act
        TestValidationResult<PublicSetPasswordCommand>? result = await _validator.TestValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor(x => x.Password);
    }

    #endregion
}
