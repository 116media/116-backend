using _116.Identity.Application.Auth.UseCases.Public.Commands.SignOut;
using _116.Identity.Application.Shared.Errors.Messages;
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
    private readonly ValidationErrorMessage _i18n = LocalizerFactory.CreateMessage<ValidationErrorMessage>();
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
        result.ShouldHaveValidationErrorFor(x => x.RefreshToken).WithErrorMessage(_i18n.RefreshTokenRequired());
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
        result.ShouldHaveValidationErrorFor(x => x.RefreshToken).WithErrorMessage(_i18n.RefreshTokenRequired());
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
        var validator = new PublicSignOutValidator(i18n);
        var command = new PublicSignOutCommand(UserId: Guid.NewGuid(), RefreshToken: "");

        // Act
        TestValidationResult<PublicSignOutCommand>? result = await validator.TestValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor(x => x.RefreshToken).WithErrorMessage(i18n.RefreshTokenRequired());
    }

    #endregion
}
