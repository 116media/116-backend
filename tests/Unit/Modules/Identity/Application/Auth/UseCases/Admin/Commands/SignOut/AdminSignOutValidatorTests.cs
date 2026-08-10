using _116.Identity.Application.Auth.UseCases.Admin.Commands.SignOut;
using _116.Identity.Application.Shared.Errors.Facade;
using _116.Tests.Fixtures.Constants;
using _116.Tests.Fixtures.Helpers;
using AwesomeAssertions;
using FluentValidation.TestHelper;
using Xunit;

namespace _116.Unit.Tests.Modules.Identity.Application.Auth.UseCases.Admin.Commands.SignOut;

/// <summary>
/// Unit tests for <see cref="AdminSignOutValidator"/>.
/// </summary>
public class AdminSignOutValidatorTests
{
    private readonly IdentityI18n _i18n = TestErrorsFactory.CreateIdentityI18n();
    private readonly AdminSignOutValidator _validator;

    /// <summary>
    /// Initializes a new instance of <see cref="AdminSignOutValidatorTests"/>.
    /// </summary>
    public AdminSignOutValidatorTests()
    {
        _validator = new AdminSignOutValidator(_i18n);
    }

    #region Valid Command Tests

    [Fact]
    public async Task Validate_WithValidCommand_ShouldNotHaveErrors()
    {
        // Arrange
        AdminSignOutCommand command = new(
            UserId: Guid.NewGuid(),
            RefreshToken: TestConstants.Session.DefaultRefreshToken
        );

        // Act
        TestValidationResult<AdminSignOutCommand>? result = await _validator.TestValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    #endregion

    #region RefreshToken Validation Tests

    [Fact]
    public async Task Validate_WithNullRefreshToken_ShouldHaveError()
    {
        // Arrange
        AdminSignOutCommand command = new(UserId: Guid.NewGuid(), RefreshToken: null!);

        // Act
        TestValidationResult<AdminSignOutCommand>? result = await _validator.TestValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .ShouldHaveValidationErrorFor(x => x.RefreshToken)
            .WithErrorMessage(_i18n.User.Validation.RefreshTokenRequired());
    }

    [Fact]
    public async Task Validate_WithEmptyRefreshToken_ShouldHaveError()
    {
        // Arrange
        AdminSignOutCommand command = new(UserId: Guid.NewGuid(), RefreshToken: string.Empty);

        // Act
        TestValidationResult<AdminSignOutCommand>? result = await _validator.TestValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .ShouldHaveValidationErrorFor(x => x.RefreshToken)
            .WithErrorMessage(_i18n.User.Validation.RefreshTokenRequired());
    }

    [Fact]
    public async Task Validate_WithWhitespaceRefreshToken_ShouldHaveError()
    {
        // Arrange
        AdminSignOutCommand command = new(UserId: Guid.NewGuid(), RefreshToken: "   ");

        // Act
        TestValidationResult<AdminSignOutCommand>? result = await _validator.TestValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .ShouldHaveValidationErrorFor(x => x.RefreshToken)
            .WithErrorMessage(_i18n.User.Validation.RefreshTokenRequired());
    }

    #endregion
}
