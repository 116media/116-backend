using _116.Identity.Application.Session.UseCases.Admin.Commands.ForceLogoutUser;
using AwesomeAssertions;
using FluentValidation.TestHelper;
using Xunit;

namespace _116.Unit.Tests.Modules.Identity.Application.Session.UseCases.Admin.Commands.ForceLogoutUser;

/// <summary>
/// Unit tests for <see cref="AdminForceLogoutUserValidator"/>.
/// </summary>
public class AdminForceLogoutUserValidatorTests
{
    private readonly AdminForceLogoutUserValidator _validator = new();

    #region Valid Command Tests

    [Fact]
    public async Task Validate_WithValidCommand_ShouldNotHaveErrors()
    {
        // Arrange
        AdminForceLogoutUserCommand command = new(UserId: Guid.NewGuid().ToString());

        // Act
        var result = await _validator.TestValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    #endregion

    #region UserId Validation Tests

    [Fact]
    public async Task Validate_WithNullUserId_ShouldHaveError()
    {
        // Arrange
        AdminForceLogoutUserCommand command = new(UserId: null!);

        // Act
        var result = await _validator.TestValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor(x => x.UserId).WithErrorMessage("User ID is required.");
    }

    [Fact]
    public async Task Validate_WithEmptyUserId_ShouldHaveError()
    {
        // Arrange
        AdminForceLogoutUserCommand command = new(UserId: string.Empty);

        // Act
        var result = await _validator.TestValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor(x => x.UserId).WithErrorMessage("User ID is required.");
    }

    [Fact]
    public async Task Validate_WithWhitespaceUserId_ShouldHaveError()
    {
        // Arrange
        AdminForceLogoutUserCommand command = new(UserId: "   ");

        // Act
        var result = await _validator.TestValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor(x => x.UserId).WithErrorMessage("User ID is required.");
    }

    [Fact]
    public async Task Validate_WithInvalidGuidFormat_ShouldHaveError()
    {
        // Arrange
        AdminForceLogoutUserCommand command = new(UserId: "not-a-guid");

        // Act
        var result = await _validator.TestValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor(x => x.UserId).WithErrorMessage("User ID is invalid.");
    }

    #endregion
}
