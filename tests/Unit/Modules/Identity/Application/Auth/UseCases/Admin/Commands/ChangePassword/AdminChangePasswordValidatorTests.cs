using _116.BuildingBlocks.Constants;
using _116.Identity.Application.Auth.UseCases.Admin.Commands.ChangePassword;
using _116.Identity.Application.Shared.Errors.Facade;
using _116.Tests.Fixtures.Constants;
using _116.Tests.Fixtures.Helpers;
using AwesomeAssertions;
using FluentValidation.TestHelper;
using Xunit;

namespace _116.Unit.Tests.Modules.Identity.Application.Auth.UseCases.Admin.Commands.ChangePassword;

/// <summary>
/// Unit tests for <see cref="AdminChangePasswordValidator"/>.
/// </summary>
public class AdminChangePasswordValidatorTests
{
    private readonly IdentityI18n _i18n = TestErrorsFactory.CreateIdentityI18n();
    private readonly AdminChangePasswordValidator _validator;

    /// <summary>
    /// Initializes a new instance of <see cref="AdminChangePasswordValidatorTests"/>.
    /// </summary>
    public AdminChangePasswordValidatorTests()
    {
        _validator = new AdminChangePasswordValidator(_i18n);
    }

    #region Valid Command Tests

    [Fact]
    public async Task Validate_WithValidCommand_ShouldNotHaveErrors()
    {
        // Arrange
        AdminChangePasswordCommand command = new(
            UserId: Guid.NewGuid(),
            SessionId: Guid.NewGuid(),
            OldPassword: TestConstants.User.ValidPassword,
            NewPassword: "NewSecure1Pass"
        );

        // Act
        TestValidationResult<AdminChangePasswordCommand>? result = await _validator.TestValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    #endregion

    #region OldPassword Validation Tests

    [Fact]
    public async Task Validate_WithNullOldPassword_ShouldHaveError()
    {
        // Arrange
        AdminChangePasswordCommand command = new(
            UserId: Guid.NewGuid(),
            SessionId: Guid.NewGuid(),
            OldPassword: null!,
            NewPassword: "NewSecure1Pass"
        );

        // Act
        TestValidationResult<AdminChangePasswordCommand>? result = await _validator.TestValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .ShouldHaveValidationErrorFor(x => x.OldPassword)
            .WithErrorMessage(_i18n.User.Validation.CurrentPasswordRequired());
    }

    [Fact]
    public async Task Validate_WithEmptyOldPassword_ShouldHaveError()
    {
        // Arrange
        AdminChangePasswordCommand command = new(
            UserId: Guid.NewGuid(),
            SessionId: Guid.NewGuid(),
            OldPassword: string.Empty,
            NewPassword: "NewSecure1Pass"
        );

        // Act
        TestValidationResult<AdminChangePasswordCommand>? result = await _validator.TestValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor(x => x.OldPassword);
    }

    #endregion

    #region NewPassword Validation Tests

    [Fact]
    public async Task Validate_WithNullNewPassword_ShouldHaveError()
    {
        // Arrange
        AdminChangePasswordCommand command = new(
            UserId: Guid.NewGuid(),
            SessionId: Guid.NewGuid(),
            OldPassword: TestConstants.User.ValidPassword,
            NewPassword: null!
        );

        // Act
        TestValidationResult<AdminChangePasswordCommand>? result = await _validator.TestValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor(x => x.NewPassword);
    }

    [Fact]
    public async Task Validate_WithTooShortNewPassword_ShouldHaveError()
    {
        // Arrange
        AdminChangePasswordCommand command = new(
            UserId: Guid.NewGuid(),
            SessionId: Guid.NewGuid(),
            OldPassword: TestConstants.User.ValidPassword,
            NewPassword: "Pass1"
        );

        // Act
        TestValidationResult<AdminChangePasswordCommand>? result = await _validator.TestValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .ShouldHaveValidationErrorFor(x => x.NewPassword)
            .WithErrorMessage(_i18n.User.Validation.PasswordTooShort("New password", UserConstants.MinPasswordLength));
    }

    [Fact]
    public async Task Validate_WithNewPasswordMissingLowercase_ShouldHaveError()
    {
        // Arrange
        AdminChangePasswordCommand command = new(
            UserId: Guid.NewGuid(),
            SessionId: Guid.NewGuid(),
            OldPassword: TestConstants.User.ValidPassword,
            NewPassword: "PASSWORD123"
        );

        // Act
        TestValidationResult<AdminChangePasswordCommand>? result = await _validator.TestValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor(x => x.NewPassword);
    }

    [Fact]
    public async Task Validate_WithNewPasswordMissingUppercase_ShouldHaveError()
    {
        // Arrange
        AdminChangePasswordCommand command = new(
            UserId: Guid.NewGuid(),
            SessionId: Guid.NewGuid(),
            OldPassword: TestConstants.User.ValidPassword,
            NewPassword: "password123"
        );

        // Act
        TestValidationResult<AdminChangePasswordCommand>? result = await _validator.TestValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor(x => x.NewPassword);
    }

    [Fact]
    public async Task Validate_WithNewPasswordMissingNumber_ShouldHaveError()
    {
        // Arrange
        AdminChangePasswordCommand command = new(
            UserId: Guid.NewGuid(),
            SessionId: Guid.NewGuid(),
            OldPassword: TestConstants.User.ValidPassword,
            NewPassword: "PasswordOnly"
        );

        // Act
        TestValidationResult<AdminChangePasswordCommand>? result = await _validator.TestValidateAsync(command);

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
        AdminChangePasswordCommand command = new(
            UserId: Guid.NewGuid(),
            SessionId: Guid.NewGuid(),
            OldPassword: string.Empty,
            NewPassword: "weak"
        );

        // Act
        TestValidationResult<AdminChangePasswordCommand>? result = await _validator.TestValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCountGreaterThanOrEqualTo(2);
        result.ShouldHaveValidationErrorFor(x => x.OldPassword);
        result.ShouldHaveValidationErrorFor(x => x.NewPassword);
    }

    #endregion
}
