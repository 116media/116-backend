using _116.BuildingBlocks.Constants;
using _116.Identity.Application.Auth.UseCases.Public.Commands.ChangePassword;
using _116.Unit.Tests.Common.Constants;
using AwesomeAssertions;
using FluentValidation.TestHelper;
using Xunit;

namespace _116.Unit.Tests.Modules.Identity.Application.Auth.UseCases.Public.Commands.ChangePassword;

/// <summary>
/// Unit tests for <see cref="PublicChangePasswordValidator"/>.
/// </summary>
public class PublicChangePasswordValidatorTests
{
    private readonly PublicChangePasswordValidator _validator = new();

    #region Valid Command Tests

    [Fact]
    public async Task Validate_WithValidCommand_ShouldNotHaveErrors()
    {
        // Arrange
        PublicChangePasswordCommand command = new(
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
        PublicChangePasswordCommand command = new(
            UserId: Guid.NewGuid(),
            SessionId: Guid.NewGuid(),
            OldPassword: null!,
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
        PublicChangePasswordCommand command = new(
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
        PublicChangePasswordCommand command = new(
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
    public async Task Validate_WithWeakNewPassword_ShouldHaveError()
    {
        // Arrange
        PublicChangePasswordCommand command = new(
            UserId: Guid.NewGuid(),
            SessionId: Guid.NewGuid(),
            OldPassword: TestConstants.User.ValidPassword,
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
        PublicChangePasswordCommand command = new(
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
    }

    #endregion
}
