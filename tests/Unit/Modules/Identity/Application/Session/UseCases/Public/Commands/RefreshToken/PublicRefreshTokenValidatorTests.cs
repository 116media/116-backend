using _116.Identity.Application.Session.UseCases.Public.Commands.RefreshToken;
using _116.Tests.Fixtures.Constants;
using AwesomeAssertions;
using FluentValidation.TestHelper;
using Xunit;

namespace _116.Unit.Tests.Modules.Identity.Application.Session.UseCases.Public.Commands.RefreshToken;

/// <summary>
/// Unit tests for <see cref="PublicRefreshTokenValidator"/>.
/// </summary>
public class PublicRefreshTokenValidatorTests
{
    private readonly PublicRefreshTokenValidator _validator = new();

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
        result.ShouldHaveValidationErrorFor(x => x.RefreshToken).WithErrorMessage("Refresh token is required.");
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
        result.ShouldHaveValidationErrorFor(x => x.RefreshToken).WithErrorMessage("Refresh token is required.");
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
        result.ShouldHaveValidationErrorFor(x => x.RefreshToken).WithErrorMessage("Refresh token is required.");
    }

    #endregion
}
