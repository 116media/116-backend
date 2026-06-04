using System.Globalization;
using _116.Identity.Application.Session.UseCases.Public.Commands.RefreshToken;
using _116.Identity.Application.Shared.Errors.Facade;
using _116.Identity.Application.Shared.Errors.Messages;
using _116.Tests.Fixtures.Constants;
using _116.Tests.Fixtures.Helpers;
using AwesomeAssertions;
using FluentValidation.TestHelper;
using Xunit;

namespace _116.Unit.Tests.Modules.Identity.Application.Session.UseCases.Public.Commands.RefreshToken;

/// <summary>
/// Unit tests for <see cref="PublicRefreshTokenValidator"/>.
/// </summary>
public class PublicRefreshTokenValidatorTests
{
    private readonly IdentityI18n _i18n = TestErrorsFactory.CreateIdentityI18n();
    private readonly PublicRefreshTokenValidator _validator;

    /// <summary>
    /// Initializes a new instance of <see cref="PublicRefreshTokenValidatorTests"/>.
    /// </summary>
    public PublicRefreshTokenValidatorTests()
    {
        _validator = new PublicRefreshTokenValidator(_i18n);
    }

    #region Valid Command Tests

    [Fact]
    public async Task Validate_WithValidCommand_ShouldNotHaveErrors()
    {
        // Arrange
        PublicRefreshTokenCommand command = new(RefreshToken: TestConstants.Session.DefaultRefreshToken);

        // Act
        TestValidationResult<PublicRefreshTokenCommand>? result = await _validator.TestValidateAsync(command);

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
        PublicRefreshTokenCommand command = new(RefreshToken: null!);

        // Act
        TestValidationResult<PublicRefreshTokenCommand>? result = await _validator.TestValidateAsync(command);

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
        PublicRefreshTokenCommand command = new(RefreshToken: string.Empty);

        // Act
        TestValidationResult<PublicRefreshTokenCommand>? result = await _validator.TestValidateAsync(command);

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
        PublicRefreshTokenCommand command = new(RefreshToken: "   ");

        // Act
        TestValidationResult<PublicRefreshTokenCommand>? result = await _validator.TestValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .ShouldHaveValidationErrorFor(x => x.RefreshToken)
            .WithErrorMessage(_i18n.User.Validation.RefreshTokenRequired());
    }

    #endregion

    #region Culture Tests

    [Theory]
    [InlineData("en")]
    [InlineData("fr")]
    public async Task Validate_ErrorMessages_ShouldBeLocalizedForCulture(string culture)
    {
        // Arrange
        Thread.CurrentThread.CurrentCulture = new CultureInfo(culture);
        Thread.CurrentThread.CurrentUICulture = new CultureInfo(culture);
        var i18n = TestErrorsFactory.CreateIdentityI18n();
        var validator = new PublicRefreshTokenValidator(i18n);
        var command = new PublicRefreshTokenCommand(RefreshToken: "");

        // Act
        TestValidationResult<PublicRefreshTokenCommand>? result = await validator.TestValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .ShouldHaveValidationErrorFor(x => x.RefreshToken)
            .WithErrorMessage(i18n.User.Validation.RefreshTokenRequired());
    }

    #endregion
}
