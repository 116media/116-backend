using _116.Identity.Application.Auth.UseCases.Public.Commands.SignOut;
using _116.Identity.Application.Shared.Errors.Facade;
using _116.Tests.Fixtures.Constants;
using _116.Tests.Fixtures.Helpers;
using AwesomeAssertions;
using FluentValidation.TestHelper;
using Xunit;

namespace _116.Unit.Tests.Modules.Identity.Application.Auth.UseCases.Public.Commands.SignOut;

/// <summary>
/// Unit tests for <see cref="PublicSignOutValidator"/>.
/// </summary>
public class PublicSignOutValidatorTests
{
    private readonly IdentityI18n _i18n = TestErrorsFactory.CreateIdentityI18n();
    private readonly PublicSignOutValidator _validator;

    /// <summary>
    /// Initializes a new instance of <see cref="PublicSignOutValidatorTests"/>.
    /// </summary>
    public PublicSignOutValidatorTests()
    {
        _validator = new PublicSignOutValidator(_i18n);
    }

    #region Valid Command Tests

    [Fact]
    public async Task Validate_WithValidCommand_ShouldNotHaveErrors()
    {
        // Arrange
        PublicSignOutCommand command = new(
            UserId: Guid.NewGuid(),
            RefreshToken: TestConstants.Session.DefaultRefreshToken
        );

        // Act
        TestValidationResult<PublicSignOutCommand>? result = await _validator.TestValidateAsync(command);

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
        PublicSignOutCommand command = new(UserId: Guid.NewGuid(), RefreshToken: null!);

        // Act
        TestValidationResult<PublicSignOutCommand>? result = await _validator.TestValidateAsync(command);

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
        PublicSignOutCommand command = new(UserId: Guid.NewGuid(), RefreshToken: string.Empty);

        // Act
        TestValidationResult<PublicSignOutCommand>? result = await _validator.TestValidateAsync(command);

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
        PublicSignOutCommand command = new(UserId: Guid.NewGuid(), RefreshToken: "   ");

        // Act
        TestValidationResult<PublicSignOutCommand>? result = await _validator.TestValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .ShouldHaveValidationErrorFor(x => x.RefreshToken)
            .WithErrorMessage(_i18n.User.Validation.RefreshTokenRequired());
    }

    #endregion
}
