using _116.Identity.Application.Session.UseCases.Admin.Commands.RefreshToken;
using _116.Identity.Application.Shared.Errors.Messages;
using _116.Tests.Fixtures.Constants;
using _116.Tests.Fixtures.Helpers;
using AwesomeAssertions;
using FluentValidation.TestHelper;
using Xunit;

namespace _116.Unit.Tests.Modules.Identity.Application.Session.UseCases.Admin.Commands.RefreshToken;

/// <summary>
/// Unit tests for <see cref="AdminRefreshTokenValidator"/>.
/// </summary>
public class AdminRefreshTokenValidatorTests
{
    private readonly ValidationErrorMessage _i18n = LocalizerFactory.CreateMessage<ValidationErrorMessage>();
    private readonly AdminRefreshTokenValidator _validator;

    /// <summary>
    /// Initializes a new instance of <see cref="AdminRefreshTokenValidatorTests"/>.
    /// </summary>
    public AdminRefreshTokenValidatorTests()
    {
        _validator = new AdminRefreshTokenValidator(_i18n);
    }

    #region Valid Command Tests

    [Fact]
    public async Task Validate_WithValidCommand_ShouldNotHaveErrors()
    {
        // Arrange
        AdminRefreshTokenCommand command = new(RefreshToken: TestConstants.Session.DefaultRefreshToken);

        // Act
        TestValidationResult<AdminRefreshTokenCommand>? result = await _validator.TestValidateAsync(command);

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
        AdminRefreshTokenCommand command = new(RefreshToken: null!);

        // Act
        TestValidationResult<AdminRefreshTokenCommand>? result = await _validator.TestValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor(x => x.RefreshToken).WithErrorMessage(_i18n.RefreshTokenRequired());
    }

    [Fact]
    public async Task Validate_WithEmptyRefreshToken_ShouldHaveError()
    {
        // Arrange
        AdminRefreshTokenCommand command = new(RefreshToken: string.Empty);

        // Act
        TestValidationResult<AdminRefreshTokenCommand>? result = await _validator.TestValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor(x => x.RefreshToken).WithErrorMessage(_i18n.RefreshTokenRequired());
    }

    [Fact]
    public async Task Validate_WithWhitespaceRefreshToken_ShouldHaveError()
    {
        // Arrange
        AdminRefreshTokenCommand command = new(RefreshToken: "   ");

        // Act
        TestValidationResult<AdminRefreshTokenCommand>? result = await _validator.TestValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor(x => x.RefreshToken).WithErrorMessage(_i18n.RefreshTokenRequired());
    }

    #endregion

    #region Culture Tests

    [Theory]
    [InlineData("en")]
    [InlineData("fr")]
    public async Task Validate_ErrorMessages_ShouldBeLocalizedForCulture(string culture)
    {
        // Arrange
        var i18n = LocalizerFactory.CreateMessage<ValidationErrorMessage>(culture);
        var validator = new AdminRefreshTokenValidator(i18n);
        var command = new AdminRefreshTokenCommand(RefreshToken: "");

        // Act
        TestValidationResult<AdminRefreshTokenCommand>? result = await validator.TestValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor(x => x.RefreshToken).WithErrorMessage(i18n.RefreshTokenRequired());
    }

    #endregion
}
