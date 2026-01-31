using _116.BuildingBlocks.Constants;
using _116.Identity.Application.Auth.UseCases.Admin.Commands.ChangePassword;
using _116.Unit.Tests.Common.Constants;
using AwesomeAssertions;
using FluentValidation.TestHelper;
using Xunit;

namespace _116.Unit.Tests.Modules.Identity.Application.Auth.UseCases.Admin.Commands.ChangePassword;

/// <summary>
/// Unit tests for <see cref="AdminChangePasswordValidator"/>.
/// </summary>
public class AdminChangePasswordValidatorTests
{
    private readonly AdminChangePasswordValidator _validator = new();

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
        var result = await _validator.TestValidateAsync(command);

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
        var result = await _validator.TestValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor(x => x.OldPassword).WithErrorMessage("Current password is required");
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
        var result = await _validator.TestValidateAsync(command);

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
        var result = await _validator.TestValidateAsync(command);

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
        var result = await _validator.TestValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .ShouldHaveValidationErrorFor(x => x.NewPassword)
            .WithErrorMessage($"New password must be at least {UserConstants.MinPasswordLength} characters long");
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
        var result = await _validator.TestValidateAsync(command);

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
        var result = await _validator.TestValidateAsync(command);

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
        AdminChangePasswordCommand command = new(
            UserId: Guid.NewGuid(),
            SessionId: Guid.NewGuid(),
            OldPassword: string.Empty,
            NewPassword: "weak"
        );

        // Act
        var result = await _validator.TestValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Count.Should().BeGreaterThanOrEqualTo(2);
        result.ShouldHaveValidationErrorFor(x => x.OldPassword);
        result.ShouldHaveValidationErrorFor(x => x.NewPassword);
    }

    #endregion
}
