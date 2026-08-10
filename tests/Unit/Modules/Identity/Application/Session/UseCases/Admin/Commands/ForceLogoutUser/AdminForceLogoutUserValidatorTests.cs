using _116.Identity.Application.Session.UseCases.Admin.Commands.ForceLogoutUser;
using _116.Identity.Application.Shared.Errors.Facade;
using _116.Tests.Fixtures.Helpers;
using AwesomeAssertions;
using FluentValidation.TestHelper;
using Xunit;

namespace _116.Unit.Tests.Modules.Identity.Application.Session.UseCases.Admin.Commands.ForceLogoutUser;

/// <summary>
/// Unit tests for <see cref="AdminForceLogoutUserValidator"/>.
/// </summary>
public class AdminForceLogoutUserValidatorTests
{
    private readonly IdentityI18n _i18n = TestErrorsFactory.CreateIdentityI18n();
    private readonly AdminForceLogoutUserValidator _validator;

    /// <summary>
    /// Initializes a new instance of <see cref="AdminForceLogoutUserValidatorTests"/>.
    /// </summary>
    public AdminForceLogoutUserValidatorTests()
    {
        _validator = new AdminForceLogoutUserValidator(_i18n);
    }

    #region Valid Command Tests

    [Fact]
    public async Task Validate_WithValidCommand_ShouldNotHaveErrors()
    {
        // Arrange
        AdminForceLogoutUserCommand command = new(UserId: Guid.NewGuid().ToString());

        // Act
        TestValidationResult<AdminForceLogoutUserCommand>? result = await _validator.TestValidateAsync(command);

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
        TestValidationResult<AdminForceLogoutUserCommand>? result = await _validator.TestValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .ShouldHaveValidationErrorFor(x => x.UserId)
            .WithErrorMessage(_i18n.User.Validation.Localizer["UserIdRequired"].Value);
    }

    [Fact]
    public async Task Validate_WithEmptyUserId_ShouldHaveError()
    {
        // Arrange
        AdminForceLogoutUserCommand command = new(UserId: string.Empty);

        // Act
        TestValidationResult<AdminForceLogoutUserCommand>? result = await _validator.TestValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .ShouldHaveValidationErrorFor(x => x.UserId)
            .WithErrorMessage(_i18n.User.Validation.Localizer["UserIdRequired"].Value);
    }

    [Fact]
    public async Task Validate_WithWhitespaceUserId_ShouldHaveError()
    {
        // Arrange
        AdminForceLogoutUserCommand command = new(UserId: "   ");

        // Act
        TestValidationResult<AdminForceLogoutUserCommand>? result = await _validator.TestValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .ShouldHaveValidationErrorFor(x => x.UserId)
            .WithErrorMessage(_i18n.User.Validation.Localizer["UserIdRequired"].Value);
    }

    [Fact]
    public async Task Validate_WithInvalidGuidFormat_ShouldHaveError()
    {
        // Arrange
        AdminForceLogoutUserCommand command = new(UserId: "not-a-guid");

        // Act
        TestValidationResult<AdminForceLogoutUserCommand>? result = await _validator.TestValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .ShouldHaveValidationErrorFor(x => x.UserId)
            .WithErrorMessage(_i18n.User.Validation.Localizer["UserIdInvalid"].Value);
    }

    #endregion
}
